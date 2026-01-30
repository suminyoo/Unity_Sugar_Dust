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

    private Dictionary<CustomerType, int> spawnProbabilities = new Dictionary<CustomerType, int>()
    {
        { CustomerType.Normal_Exact, 25 },
        { CustomerType.Normal_BigBill, 25 },
        { CustomerType.Scammer, 20 },
        { CustomerType.Haggler, 15 },
        { CustomerType.Impatient, 5 },
        { CustomerType.CoinOnly, 5 },
        { CustomerType.Disturber, 3 },
        { CustomerType.Beggar, 1 },
        { CustomerType.Tipper, 1 }
    };

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
        NPCVisualUtility.ApplyRandomColor(customer);

        if (newCustomer != null)
        {
            currentCustomers.Add(newCustomer);
            CustomerType selectedType = GetWeightedRandomType();
            float randomStay = Random.Range(shopStayDurationMin, shopStayDurationMax);

            newCustomer.Setup(
                displayStand,
                counter,
                spawnPoint,
                selectedType,
                randomStay,
                () => {
                    if (currentCustomers.Contains(newCustomer))
                        currentCustomers.Remove(newCustomer);
                    Destroy(customer);
                }
            );
        }
    }

    private CustomerType GetWeightedRandomType()
    {
        int totalWeight = 0;
        foreach (var kvp in spawnProbabilities) totalWeight += kvp.Value;

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var kvp in spawnProbabilities)
        {
            currentWeight += kvp.Value;
            if (randomValue < currentWeight)
            {
                return kvp.Key;
            }
        }
        return CustomerType.Normal_BigBill;
    }
}