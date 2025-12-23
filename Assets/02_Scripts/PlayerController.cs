using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 15f;

    public float heavySpeed = 3f;
    public float tooHeavySpeed = 1f;

    [Header("References")]
    public Transform cameraTransform;
    public Transform characterModel;
    private InventorySystem inventory;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isInteracting = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        inventory = GetComponent<InventorySystem>();
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        //입력 처리
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        //카메라 기준 이동 방향 계산
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0; camRight.y = 0;

        moveInput = (camForward.normalized * v + camRight.normalized * h).normalized;

        //마우스 상호작용 체크
        isInteracting = Input.GetMouseButton(0) || Input.GetMouseButton(1);

        //상호작용 중이면 마우스 방향 보기, 아니면 이동 방향 보기
        if (isInteracting) 
        {
            LookAtMouse();
        }
        else if (moveInput != Vector3.zero) 
        {
            LookAtMoveDirection();
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    // 이동 방향을 바라보기
    void LookAtMoveDirection()
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveInput);
        characterModel.rotation = Quaternion.Slerp(characterModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // 마우스 지점 바라보기
    void LookAtMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 lookPoint = ray.GetPoint(rayDistance);
            Vector3 direction = (lookPoint - transform.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                characterModel.rotation = Quaternion.Slerp(characterModel.rotation, targetRotation, rotationSpeed * 1.5f * Time.deltaTime);
            }
        }
    }

    void MovePlayer()
    {
        // 무게 비율 (0.0 ~ 1.0 이상)
        float currentWeight = inventory != null ? inventory.currentWeight : 0f;
        float maxWeight = inventory != null ? inventory.maxWeight : 100f;

        if (maxWeight == 0) maxWeight = 1f;

        float weightRatio = currentWeight / maxWeight;
        float finalSpeed = moveSpeed;

        // 과적
        if (weightRatio >= 1.0f)
        {
            Debug.Log("가방이 너무 무겁다..");
            finalSpeed = tooHeavySpeed;
        }
        // 최대 수용 가능 무게의 80퍼
        else if (weightRatio >= 0.8f)
        {
            Debug.Log("가방이 무겁다..");
            finalSpeed = heavySpeed;
        }

        // 이동
        if (moveInput != Vector3.zero)
        {
            rb.MovePosition(rb.position + moveInput * finalSpeed * Time.fixedDeltaTime);

            // 회전
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            characterModel.rotation = Quaternion.Slerp(characterModel.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        // 애니메이션 속도 조절 (뛰다가 걷다가 기어가게 보이도록)
        // 걷는 애니메이션 속도를 실제 이동 속도에 맞춰
        //if (animator != null)
        //{
        //    float animSpeed = finalSpeed / moveSpeed; // 1.0(정상) ~ 0.2(느림)
        //    animator.speed = animSpeed;
        //    animator.SetFloat("MoveSpeed", moveInput.magnitude * animSpeed);
        //}
    }
}