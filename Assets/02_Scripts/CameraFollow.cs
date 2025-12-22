using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float rotateSpeed = 200f;

    private float currentRotationAngle = 0f;

    void Start()
    {
        if (target == null) return;
        currentRotationAngle = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetMouseButton(2))
        {
            currentRotationAngle += Input.GetAxis("Mouse X") * rotateSpeed * Time.unscaledDeltaTime;
        }

        Quaternion rotation = Quaternion.Euler(0, currentRotationAngle, 0);
        Vector3 targetPosition = target.position + (rotation * offset);

        transform.position = targetPosition;

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}