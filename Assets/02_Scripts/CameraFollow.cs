using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(-10, 10, 5);
    public float rotateSpeed = 200f;
    public float smoothTime = 0.05f;

    private float currentXAngle = 0f;
    private float currentYAngle = 30f;
    private Vector3 currentVelocity = Vector3.zero;

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
            currentXAngle += Input.GetAxis("Mouse X") * rotateSpeed * Time.unscaledDeltaTime;
            currentYAngle -= Input.GetAxis("Mouse Y") * rotateSpeed * Time.unscaledDeltaTime;
            currentYAngle = Mathf.Clamp(currentYAngle, 10f, 80f);
        }
    }

    void FixedUpdate() 
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(currentYAngle, currentXAngle, 0);
        float distance = offset.magnitude;
        Vector3 rotatedOffset = rotation * new Vector3(0, 0, -distance);
        Vector3 targetPosition = target.position + rotatedOffset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime, Mathf.Infinity, Time.fixedDeltaTime);
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}