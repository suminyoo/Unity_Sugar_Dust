using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform mainCameraTransform;

    private void Start()
    {
        if (Camera.main != null)
            mainCameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (mainCameraTransform == null) return;

        // UI가 항상 카메라 정면을 바라보게 회전
        transform.forward = mainCameraTransform.forward;
    }
}