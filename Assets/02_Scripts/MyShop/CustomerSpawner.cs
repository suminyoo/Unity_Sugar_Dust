using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject customerPrefab;
    public Transform spawnPoint;

    [Header("Scene Objects")]
    public DisplayStand displayStand;
    public CheckoutCounter counter;

    [Header("Settings")]
    public float spawnInterval = 10f;
    public int maxCustomers = 5;

    private List<CustomerBrain> currentCustomers = new List<CustomerBrain>();

    private void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (currentCustomers.Count < maxCustomers && customerPrefab != null)
            {
                SpawnCustomer();
            }
            yield return new WaitForSeconds(spawnInterval);
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