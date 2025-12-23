using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(-10, 10, 5);
    public float rotateSpeed = 200f;
    public float smoothTime = 0.05f;

    private float currentXAngle = 0f;
    private float currentYAngle = 30f;
    private Vector3 currentVelocity = Vector3.zero; // SmoothDamp 용 변수 (변수유지해야 함수 쓸때 초기화 안됨)

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
    }

    void FixedUpdate() // 이동처리, 카메라 떨리지 않게
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(currentYAngle, currentXAngle, 0);
        float distance = offset.magnitude; //카메라와 타겟의 거리
        Vector3 rotatedOffset = rotation * new Vector3(0, 0, -distance);
        Vector3 targetPosition = target.position + rotatedOffset;

        // 카메라의 부드러운 이동, 관성 있게 smoothDamp
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        transform.LookAt(target.position + Vector3.up * 1.5f); //타겟 머리쪽 보기
    }
}