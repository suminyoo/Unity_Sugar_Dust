using UnityEngine;

public class WorldItem : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData itemData;
    public int amount = 1;

    [Header("Magnet Settings")]
    private Transform target;
    public float moveSpeed = 10f;

    [Header("Visual Effect")]
    public float rotateSpeed = 50f;
    public float floatSpeed = 2f;
    public float floatHeight = 0.25f;

    private bool isFloating = false;
    private Vector3 startPos;
    private Rigidbody rb;

    public void Initialize(ItemData data, int count)
    {
        itemData = data;
        amount = count;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        startPos = transform.position;
        isFloating = true;
    }

    public void StartFollow(Transform player)
    {
        target = player;
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
    }

    public void StopFollow(Transform player)
    {
        if (target == player) target = null;
    }

    private void Update()
    {
        // È¸Àü
        if (isFloating || target != null)
        {
            transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
        }

        // ÀÌµ¿
        if (target != null)
        {
            // ÀÚ¼® ¸ðµå
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                moveSpeed * Time.deltaTime
            );
        }
        else if (isFloating)
        {
            // µÕµÕ ¸ðµå
            float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InventorySystem inventory = other.GetComponent<InventorySystem>();
            if (inventory != null)
            {
                if (inventory.AddItem(itemData, amount))
                {
                    Debug.Log($"{itemData.itemName} È¹µæ");
                    Destroy(gameObject);
                }
            }
        }
    }
}