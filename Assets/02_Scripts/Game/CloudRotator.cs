using UnityEngine;

public class CloudRotator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 50f;
    [SerializeField] private Vector3 direction = new Vector3(0, 0, 1);

    [SerializeField] private float disappearZ = 500f;

    [SerializeField] private float startZ = -500f;

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.Self);
        if (transform.localPosition.z >= disappearZ)
        {
            Vector3 newPos = transform.localPosition;
            newPos.z = startZ;
            transform.localPosition = newPos;
        }
    }
}