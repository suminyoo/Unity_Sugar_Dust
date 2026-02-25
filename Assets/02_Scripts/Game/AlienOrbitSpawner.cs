using UnityEngine;

public class AlienOrbitSpawner : MonoBehaviour
{
    public GameObject alienPrefab;
    public int spawnCount = 30;
    public float sphereRadius = 10f;

    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * sphereRadius;

            GameObject newAlien = Instantiate(alienPrefab, randomPos, Random.rotation);
            newAlien.transform.SetParent(this.transform);

            AlienOrbit orbitScript = newAlien.GetComponent<AlienOrbit>();
            if (orbitScript != null)
            {
                orbitScript.SetCenter(transform.position);
            }
            NPCVisualUtility.ApplyRandomColor(newAlien);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }
}