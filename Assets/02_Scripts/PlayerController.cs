using UnityEngine;
using static ActionSystem;

public enum PlayerState
{
    Idle,
    Move,
    Jump,
    Wield, //공격,채광
    Damaged,
    Die
}

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float rotationSpeed = 5f;
    public float jumpForce = 5f;
    public float actionRotationSpeed = 20f;

    [Header("Weight Penalty")]
    public float heavySpeed = 3f;
    public float tooHeavySpeed = 1f;

    [Header("References")]
    public Transform cameraTransform;
    public Transform characterModel;
    public Animator animator;
    private InventorySystem inventory;
    private Rigidbody rb;

    public PlayerState currentState = PlayerState.Idle;

    private Vector3 moveInput;
    private bool isInteracting = false;
    private bool canRun = false;
    private bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        inventory = GetComponent<InventorySystem>();

        if (cameraTransform == null) cameraTransform = Camera.main.transform;
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (currentState == PlayerState.Die) return;

        HandleInput();
        UpdateStateAndAnimation();
    }

    void FixedUpdate()
    {
        if (currentState == PlayerState.Die) return;

        MovePlayer();
    }

    void HandleInput()
    {
        // 입력
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0; camRight.y = 0;

        moveInput = (camForward.normalized * v + camRight.normalized * h).normalized;

        // 달리기
        canRun = Input.GetKey(KeyCode.LeftShift) && moveInput != Vector3.zero;

        // 점프
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // 회전 (액션중, 이동중)
        isInteracting = Input.GetMouseButton(0) || Input.GetMouseButton(1);

        if (isInteracting)
        {
            LookAtMouse(actionRotationSpeed);
        }
        else if (moveInput != Vector3.zero)
        {
            LookAtMoveDirection(rotationSpeed);
        }
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        currentState = PlayerState.Jump;
        animator.SetTrigger("Jump");
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // normal.y > 0.5f는 윗면
            if (collision.contacts[0].normal.y > 0.5f)
            {
                isGrounded = true;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void MovePlayer()
    {
        // 무게 계산
        float currentWeight = inventory != null ? inventory.currentWeight : 0f;
        float maxWeight = inventory != null ? inventory.maxWeight : 100f;
        if (maxWeight == 0) maxWeight = 1f;

        float weightRatio = currentWeight / maxWeight;
        float finalSpeed = walkSpeed;

        // 무게에따른 속도
        // TODO: 무게제한 로직 수정(수식수정)
        if (weightRatio >= 1.0f) // 100퍼
        {
            finalSpeed = tooHeavySpeed;
            canRun = false;
        }
        else if (weightRatio >= 0.8f) // 80퍼
        {
            finalSpeed = heavySpeed;
            canRun = false;
        }
        else 
        {
            finalSpeed = canRun ? runSpeed : walkSpeed;
        }

        // 이동
        if (moveInput != Vector3.zero)
        {
            rb.MovePosition(rb.position + moveInput * finalSpeed * Time.fixedDeltaTime);
        }
    }
    void UpdateStateAndAnimation()
    {
        if (animator == null) return;

        if (!isGrounded)
        {
            currentState = PlayerState.Jump;
        }
        else if (moveInput != Vector3.zero)
        {
            currentState = PlayerState.Move;
        }
        else
        {
            currentState = PlayerState.Idle;
        }

        // 블렌드 트리 애니메이션

        //월드 기준 이동 방향을 캐릭터 기준으로 변환
        Vector3 localVelocity = characterModel.InverseTransformDirection(moveInput);

        // 걷기=0.5, 달리기=1.0
        float animSpeed = 0f;

        if (moveInput != Vector3.zero) // 이동일때
        {
            if (canRun) //과적아닐때
                animSpeed = 1.0f;
            else
                animSpeed = 0.5f;
        }

        animator.SetFloat("InputX", localVelocity.x * animSpeed, 0.1f, Time.deltaTime);
        animator.SetFloat("InputY", localVelocity.z * animSpeed, 0.1f, Time.deltaTime);

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsRunning", canRun);
    }

    public void OnTakeDamage()
    {
        currentState = PlayerState.Damaged;
        animator.SetTrigger("Hit");
    }

    public void OnWield(ActionType actionType)
    {
        if (!isGrounded) return;

        currentState = PlayerState.Wield;

        // 공격 채광 동일 모션
        animator.SetTrigger("Wield");

        // 분기점
        // if (actionType == ActionType.Attack) ...
    }


    public void OnDie()
    {
        currentState = PlayerState.Die;
        animator.SetTrigger("Die");
    }

    void LookAtMoveDirection(float speed)
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveInput);
        characterModel.rotation =
            Quaternion.Slerp(characterModel.rotation, targetRotation, speed * Time.deltaTime);
    }

    void LookAtMouse(float speed)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 lookPoint = ray.GetPoint(rayDistance);
            Vector3 direction = (lookPoint - transform.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                characterModel.rotation =
                    Quaternion.Slerp(characterModel.rotation, targetRotation, speed * Time.deltaTime);
            }
        }
    }

}