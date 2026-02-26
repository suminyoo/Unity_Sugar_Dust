using UnityEngine;

public class AlienOrbit : MonoBehaviour
{
    public float minOrbitSpeed = 2f;
    public float maxOrbitSpeed = 10f;

    public float minSelfRotationSpeed = 10f;
    public float maxSelfRotationSpeed = 40f;

    public float minAnimSpeed = 0.8f;
    public float maxAnimSpeed = 1.2f;

    private Vector3 centerPoint;
    private Vector3 orbitAxis;
    private float orbitSpeed;

    private Vector3 selfRotationAxis;
    private float selfRotationSpeed;

    public void SetCenter(Vector3 center)
    {
        centerPoint = center;
    }
    void Start()
    {
        orbitAxis = Random.onUnitSphere;
        orbitSpeed = Random.Range(minOrbitSpeed, maxOrbitSpeed);

        selfRotationAxis = Random.onUnitSphere;
        selfRotationSpeed = Random.Range(minSelfRotationSpeed, maxSelfRotationSpeed);

        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.speed = Random.Range(minAnimSpeed, maxAnimSpeed);
            anim.Play(0, -1, Random.Range(0f, 1f));
        }
    }

    void Update()
    {
        transform.RotateAround(centerPoint, orbitAxis, orbitSpeed * Time.deltaTime);
        transform.Rotate(selfRotationAxis, selfRotationSpeed * Time.deltaTime);
    }
}