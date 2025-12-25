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

    private float currentXAngle = 0f;
    private float currentYAngle = 30f;
    private Vector3 currentVelocity = Vector3.zero; // SmoothDamp 용 변수 (변수유지해야 함수 쓸때 초기화 안됨)

    // 줌 제어 변수
    private float targetDistance;
    private float currentDistance;
    private float zoomVelocity;

    void Start()
    {
        // 초기 거리는 설정된 offset의 길이로 시작
        targetDistance = offset.magnitude;
        currentDistance = targetDistance;
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
            currentYAngle = Mathf.Clamp(currentYAngle, 10f, 80f); // 수직 각도 제한을 위한 clamp 함수
        }

        // 휠 스크롤 입력 (줌인/줌아웃)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            targetDistance -= scrollInput * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
    }

    void FixedUpdate() // 이동처리, 카메라 떨리지 않게
    {
        if (target == null) return;

        // 줌 거리 부드럽게 SmoothDamp
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref zoomVelocity, zoomSmoothTime);

        Quaternion rotation = Quaternion.Euler(currentYAngle, currentXAngle, 0);

        // 기존 offset.magnitude 대신 가변적인 currentDistance 사용
        Vector3 rotatedOffset = rotation * new Vector3(0, 0, -currentDistance);
        Vector3 targetPosition = target.position + rotatedOffset;

        // 카메라의 부드러운 이동, 관성 있게 smoothDamp
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        //transform.position = Vector3.Lerp(
        //    transform.position,
        //    targetPosition,
        //    1f - Mathf.Exp(-followSpeed * Time.deltaTime)
        //    );

        transform.LookAt(target.position + Vector3.up * 1.5f); //타겟 머리쪽 보기
    }
}