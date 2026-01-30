using UnityEngine;

public class WorldItem : MonoBehaviour
{ 
    #region Variables & Settings

    [Header("Item Data")]
    public ItemData itemData;
    public int amount = 1;

    [Header("Item Settings")]
    private Transform target;
    [SerializeField] private Transform visualChild;

    private readonly float moveSpeed = 5f;
    private readonly float rotateSpeed = 50f;
    private readonly float floatSpeed = 2f;
    private readonly float floatHeight = 0.25f;

    private float pickupDelay = 2f;
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
        if (visualChild == null && transform.childCount > 0)
            visualChild = transform.GetChild(0);

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
    }

    public void StopFollow(Transform player)
    {
        if (target == player) target = null;
        startPos = transform.position; //아이템 이동 위치 저장

    }
    #endregion

    private void Update()
    {
        if (visualChild == null) return;
        visualChild.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        if (target == null && isFloating)
        {
            float newY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            visualChild.localPosition = new Vector3(0, newY, 0);
            visualChild.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        }
        else
        {
            // 따라가는 중에는 둥둥거림을 멈추고 자식을 부모 중심으로 서서히 정렬
            visualChild.localPosition = Vector3.Lerp(visualChild.localPosition, Vector3.zero, Time.deltaTime * 5f);
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
                    //Debug.Log($"{itemData.itemName} 획득");
                    Destroy(gameObject);
                }
            }
        }
    }
}