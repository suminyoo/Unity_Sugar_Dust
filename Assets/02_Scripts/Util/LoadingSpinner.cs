using UnityEngine;

public class LoadingSpinner : MonoBehaviour
{
    public float rotateSpeed = 200f;

    void Update()
    {
        // Z축 기준으로 계속 회전
        transform.Rotate(0, 0, -rotateSpeed * Time.unscaledDeltaTime);
    }
}