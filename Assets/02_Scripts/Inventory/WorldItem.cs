using UnityEngine;

public class WorldItem : MonoBehaviour
{ 
    #region Variables & Settings

    [Header("Item Data")]
    public ItemData itemData;
    public int amount = 1;

    [Header("Item Settings")]
    private Transform target;

    private readonly float moveSpeed = 5f;
    private readonly float rotateSpeed = 50f;
    private readonly float floatSpeed = 2f;
    private readonly float floatHeight = 0.25f;

    public float pickupDelay = 2f;
    private float enablePickupTime;  // 줍기 가능 타임

    private bool isFloating = false;
    private Vector3 startPos;
    private Rigidbody rb;

    #endregion

    #region Initialization
    public void Initialize(ItemData data, int count)
    {
        itemData = data;
        amount = count;

        enablePickupTime = Time.time + pickupDelay;
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

        if (enablePickupTime == 0) enablePickupTime = Time.time;
    }
    #endregion

    #region Follow Logic
    public void StartFollow(Transform player)
    {
        if (Time.time < enablePickupTime) return;

        target = player;
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
    }

    public void StopFollow(Transform player)
    {
        if (target == player) target = null;
        startPos = transform.position; //아이템 이동 위치 저장

    }
    #endregion

    private void Update()
    {
        // 회전
        if (isFloating || target != null)
        {
            transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
        }

        // 이동
        if (target != null)
        {
            // 자석 모드
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                moveSpeed * Time.deltaTime
            );
        }
        else if (isFloating)
        {
            // 둥둥 모드
            float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < enablePickupTime) return;

        if (other.CompareTag("Player"))
        {
            InventoryHolder inventory = other.GetComponent<InventoryHolder>();

            if (inventory != null)
            {
                if (inventory.AddItem(itemData, amount))
                {
                    Debug.Log($"{itemData.itemName} 획득");
                    Destroy(gameObject);
                }
            }
        }
    }
}