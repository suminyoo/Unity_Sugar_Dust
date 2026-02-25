using UnityEngine;

public class AlienOrbit : MonoBehaviour
{
    public float minOrbitSpeed = 10f;
    public float maxOrbitSpeed = 40f;

    public string[] animationStateNames;

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
        selfRotationSpeed = Random.Range(10f, 50f);

        Animator anim = GetComponent<Animator>();
        if (anim != null && animationStateNames != null && animationStateNames.Length > 0)
        {
            int randomIndex = Random.Range(0, animationStateNames.Length);
            string selectedAnim = animationStateNames[randomIndex];

            anim.Play(selectedAnim);
            anim.speed = Random.Range(0.8f, 1.2f);
        }
    }

    void Update()
    {
        transform.RotateAround(centerPoint, orbitAxis, orbitSpeed * Time.deltaTime);
        transform.Rotate(selfRotationAxis, selfRotationSpeed * Time.deltaTime);
    }
}