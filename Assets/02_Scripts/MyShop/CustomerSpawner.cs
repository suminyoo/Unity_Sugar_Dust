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
    [SerializeField] float minSpawnInterval = 3f;
    [SerializeField] float maxSpawnInterval = 10f;
    [SerializeField] float shopStayDurationMin = 5f;
    [SerializeField] float shopStayDurationMax = 15f;
    [SerializeField] int maxCustomers = 10;

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
        GameObject customer = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
        CustomerBrain newCustomer = customer.GetComponent<CustomerBrain>();

        if (newCustomer != null)
        {
            currentCustomers.Add(newCustomer);
            CustomerType randomType = (CustomerType)Random.Range(0, System.Enum.GetValues(typeof(CustomerType)).Length);
            float randomStay = Random.Range(shopStayDurationMin, shopStayDurationMax);

            newCustomer.Setup(
                displayStand,
                counter,
                spawnPoint,
                randomType,
                randomStay,
                () => {
                    if (currentCustomers.Contains(newCustomer))
                        currentCustomers.Remove(newCustomer);
                    Destroy(customer);
                }
            );
        }
    }
}