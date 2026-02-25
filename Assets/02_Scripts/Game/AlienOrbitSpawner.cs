using UnityEngine;

public class AlienOrbitSpawner : MonoBehaviour
{
    public GameObject alienPrefab;     // 생성할 외계인 프리팹
    public int spawnCount = 30;        // 스폰할 외계인 마리 수
    public float sphereRadius = 10f;   // 외계인들이 돌아다닐 구의 반지름

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