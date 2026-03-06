using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    public Transform targetModel;

    public float amplitude = 0.5f;
    public float frequency = 1f;

    public Vector3 rotationSpeed = new Vector3(0, 0, 0);

    private Vector3 startPosition;

    void Start()
    {
        if (targetModel == null)
            targetModel = transform;

        startPosition = targetModel.localPosition;
    }

    void Update()
    {
        if (targetModel == null) return;

        float newY = startPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
        targetModel.localPosition = new Vector3(startPosition.x, newY, startPosition.z);

        targetModel.Rotate(rotationSpeed * Time.deltaTime);
    }
}