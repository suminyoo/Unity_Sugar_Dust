using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject customerPrefab;
    public Transform spawnPoint;
    public DisplayStand displayStand;
    public CheckoutCounter counter;

    [Header("Settings")]
    public float minSpawnInterval = 3f; 
    public float maxSpawnInterval = 10f;
    public float shopStayDurationMin = 5f;
    public float shopStayDurationMax = 15f;

    public int maxCustomers = 10;

    private List<CustomerBrain> currentCustomers = new List<CustomerBrain>();
    private Coroutine spawnCoroutine;

    public void StartSpawning()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 랜덤 대기
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            // 최대 인원
            if (currentCustomers.Count < maxCustomers)
            {
                SpawnCustomer();
            }
        }
    }

    private void SpawnCustomer()
    {
        GameObject go = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        CustomerBrain newCustomer = go.GetComponent<CustomerBrain>();

        if (newCustomer != null)
        {
            currentCustomers.Add(newCustomer);

            // 랜덤 성격 부여
            newCustomer.myType = (CustomerType)Random.Range(0, System.Enum.GetValues(typeof(CustomerType)).Length);
            
            // 랜덤 체류 시간
            float randomStay = Random.Range(shopStayDurationMin, shopStayDurationMax);
            newCustomer.wanderDuration = randomStay;

            // 초기화 (상점, 카운터, 입구, 파괴 콜백)
            newCustomer.Setup(displayStand, counter, spawnPoint, () =>
            {
                if (currentCustomers.Contains(newCustomer))
                    currentCustomers.Remove(newCustomer);
                Destroy(go);
            });
        }
    }
}