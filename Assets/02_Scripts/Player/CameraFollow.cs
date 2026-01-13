using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public Vector3 offset = new Vector3(-10, 10, 5);
    public float rotateSpeed = 200f;
    public float smoothTime = 0.05f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 10f;
    public float minDistance = 2f;
    public float maxDistance = 20f;
    public float zoomSmoothTime = 0.2f;

    [Header("Collision Settings")]
    public LayerMask collisionLayers; // 벽으로 인식할 레이어 (Inspector에서 꼭 설정하세요!)
    public float cameraRadius = 0.2f; // 감지할 카메라 두께
    public float wallOffset = 0.1f;   // 벽에서 살짝 띄우는 거리

    private float currentXAngle = 0f;
    private float currentYAngle = 30f;
    private Vector3 currentVelocity = Vector3.zero; // SmoothDamp 용 변수 (변수유지해야 함수 쓸때 초기화 안됨)

    private float minVerticalAngle = 10f; // 최저 각도
    private float maxVerticalAngle = 80f;  // 최고 각도

    // 줌 제어 변수
    private float targetDistance;
    private float currentDistance;
    private float zoomVelocity;

    void Start()
    {
        // 초기 거리는 설정된 offset의 길이로 시작
        targetDistance = offset.magnitude;
        currentDistance = targetDistance;
        SnapToTarget();

    }

    void Update()
    {
        // 휠 클릭 제어 (화면회전)
        if (Input.GetMouseButtonDown(2))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (Input.GetMouseButtonUp(2))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetMouseButton(2))
        {
            currentXAngle += Input.GetAxis("Mouse X") * rotateSpeed * Time.unscaledDeltaTime; //unscaledDeltaTime: timescale 무시하기위해
            currentYAngle -= Input.GetAxis("Mouse Y") * rotateSpeed * Time.unscaledDeltaTime;
            currentYAngle = Mathf.Clamp(currentYAngle, minVerticalAngle, maxVerticalAngle); // 수직 각도 제한을 위한 clamp 함수
        }

        // 휠 스크롤 입력 (줌인/줌아웃)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            targetDistance -= scrollInput * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
    }

    public void SnapToTarget()
    {
        if (target == null) return;

        // 플레이어(모델)의 Y축 회전값 + 180f => 플레이어 정면
        currentXAngle = target.eulerAngles.y + 180f;
        currentYAngle = 10f;
        targetDistance = 8f;
        

        // 내부 변수 및 물리 속도 초기화
        currentDistance = targetDistance;
        zoomVelocity = 0f;
        currentVelocity = Vector3.zero;

        Vector3 targetPos = CalculateCameraPosition(currentDistance);

        // 이동 및 회전
        transform.position = targetPos;

        Vector3 pivot = target.position + Vector3.up * 1.5f;
        transform.LookAt(pivot);

        Debug.Log("카메라 위치 동기화 완료");
    }

    // 위치 계산 로직
    private Vector3 CalculateCameraPosition(float dist)
    {
        Quaternion rotation = Quaternion.Euler(currentYAngle, currentXAngle, 0);
        Vector3 pivot = target.position + Vector3.up * 1.5f;
        Vector3 dir = rotation * Vector3.back;

        float finalDistance = dist;

        // 벽 충돌 체크
        RaycastHit hit;
        if (Physics.SphereCast(pivot, cameraRadius, dir, out hit, dist, collisionLayers))
        {
            float distToWall = hit.distance - wallOffset;
            finalDistance = Mathf.Clamp(distToWall, minDistance, dist);
        }

        return pivot + (dir * finalDistance);
    }

    void FixedUpdate() // 이동처리, 카메라 떨리지 않게
    {
        if (target == null) return;

        // 줌 거리 부드럽게 SmoothDamp
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref zoomVelocity, zoomSmoothTime);

        //회전값
        Quaternion rotation = Quaternion.Euler(currentYAngle, currentXAngle, 0);

        // 머리 높이 피봇
        Vector3 pivot = target.position + Vector3.up * 1.5f;
        Vector3 dir = rotation * Vector3.back; // 카메라뒷방향

        float finalDistance = currentDistance;

        // 플레이어 머리에서 카메라 방향으로 SphereCast
        RaycastHit hit;
        if (Physics.SphereCast(pivot, cameraRadius, dir, out hit, currentDistance, collisionLayers))
        {
            // 벽에 닿았다면 거리를 벽까지의 거리로 짧게 줄임
            // 최소 거리보다는 멀게 유지해야함
            float distToWall = hit.distance - wallOffset;
            finalDistance = Mathf.Clamp(distToWall, minDistance, currentDistance);
        }

        // 최종 위치 계산
        // pivot에서 dir으로 finalDistance만큼
        Vector3 targetPosition = pivot + (dir * finalDistance);

        // 이동
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        // 타겟 바라보기
        transform.LookAt(pivot);
    }
}