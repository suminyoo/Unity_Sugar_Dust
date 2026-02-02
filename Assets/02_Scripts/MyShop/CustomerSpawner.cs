using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CustomerType
{
    Normal_Exact,       // µü ¸ÂÃç µ· ÁöºÒ
    Normal_BigBill,     // ±Ý¾×º¸´Ù »ìÂ¦ ³ôÀº °í¾×±ÇÀ¸·Î ÁöºÒ
    Scammer,            // »ç±â²Û
    Haggler,            // ÈïÁ¤²Û
    Impatient,          // ÂüÀ»¼º ¾øÀ½
    CoinOnly,           // ÀÜµ· Áö¿Á
    Disturber,          // ¹æÇØ²Û
    Beggar,             // °ÅÁö
    Tipper              // ÆÁ ÁÖ´Â ¼Õ´Ô
}

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
    [SerializeField] float minGenerosity = 0.8f;
    [SerializeField] float maxGenerosity = 1.2f;
    [SerializeField] int maxCustomers = 10;

    private List<CustomerBrain> currentCustomers = new List<CustomerBrain>();
    private Coroutine spawnCoroutine;

    private Dictionary<CustomerType, int> spawnProbabilities = new Dictionary<CustomerType, int>()
    {
        { CustomerType.Normal_Exact, 15 },
        { CustomerType.Normal_BigBill, 20 },
        { CustomerType.Scammer, 30 },
        { CustomerType.Haggler, 10 },
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
            // ·£´ý ´ë±â
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            // ÃÖ´ë ÀÎ¿ø
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
            float randomGenerosity = Random.Range(minGenerosity, maxGenerosity);

            newCustomer.Setup(
                displayStand,
                counter,
                spawnPoint,
                selectedType,
                randomStay,
                randomGenerosity,
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