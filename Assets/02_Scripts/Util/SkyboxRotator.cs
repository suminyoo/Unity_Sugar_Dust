using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    [Header("스카이박스 설정")]
    [Range(0f, 360f)]
    public float startRotation = 0f;

    public float rotationSpeed = 1.0f;

    private float currentRotation = 0f;

    void Start()
    {
        currentRotation = startRotation;

        RenderSettings.skybox.SetFloat("_Rotation", currentRotation);
    }

    void Update()
    {
        currentRotation += rotationSpeed * Time.deltaTime;

        if (currentRotation >= 360f) currentRotation -= 360f;
        if (currentRotation <= -360f) currentRotation += 360f;

        RenderSettings.skybox.SetFloat("_Rotation", currentRotation);
    }
}