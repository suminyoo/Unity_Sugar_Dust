using UnityEngine;
using static ActionSystem;
using System;

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
    public event Action OnPlayerDied;

    private float currentHp;
    public PlayerData data;

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

        Initialize();
    }
    public void Initialize()
    {
        if (data == null)
        {
            Debug.LogError("Player Data가 없습니다!");
            return;
        }

        // 1. 스탯 초기화
        currentHp = data.maxHp;

        // 2. 상태 초기화
        currentState = PlayerState.Idle;
        isGrounded = true; // 시작할 땐 보통 땅에 있다고 가정 (필요시 레이캐스트 체크)
        isInteracting = false;
        canRun = false;

        // 3. 물리 및 컴포넌트 활성화 (Die()에서 꺼진 것들 다시 켜기)
        rb.velocity = Vector3.zero;
        rb.isKinematic = false; // 물리 다시 켜기
        this.enabled = true;    // 스크립트 다시 켜기

        // 4. 애니메이션 리셋 (선택사항: Idle로 강제 전환)
        if (animator != null)
        {
            animator.Rebind(); // 애니메이터 초기화
            animator.Update(0f);
        }

        Debug.Log("플레이어 초기화 완료");
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

        Move();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // normal.y > 0.5f는 윗면
            if (collision.contacts[0].normal.y > 0.5f)
            {
                isGrounded = true;
                //SnapToGround();
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
            LookAtMouse(data.actionRotationSpeed);
        }
        else if (moveInput != Vector3.zero)
        {
            LookAtMoveDirection(data.rotationSpeed);
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

    void Move()
    {
        // 무게 계산
        float currentWeight = inventory != null ? inventory.currentWeight : 0f;
        float maxWeight = inventory != null ? inventory.maxWeight : 100f;
        if (maxWeight == 0) maxWeight = 1f;

        float weightRatio = currentWeight / maxWeight;
        float finalSpeed = data.walkSpeed;

        // 무게에따른 속도
        // TODO: 무게제한 로직 수정(수식수정)
        if (weightRatio >= 1.0f) // 100퍼
        {
            finalSpeed = data.tooHeavySpeed;
            canRun = false;
        }
        else if (weightRatio >= 0.8f) // 80퍼
        {
            finalSpeed = data.heavySpeed;
            canRun = false;
        }
        else 
        {
            finalSpeed = canRun ? data.runSpeed : data.walkSpeed;
        }

        // 이동
        if (moveInput != Vector3.zero)
        {
            rb.MovePosition(rb.position + moveInput * finalSpeed * Time.fixedDeltaTime);
        }
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * data.jumpForce, ForceMode.Impulse);
        isGrounded = false;
        currentState = PlayerState.Jump;
        animator.SetTrigger("Jump");
    }

    public void TakeDamage(float damage)
    {
        if (currentState == PlayerState.Die) return;

        currentHp -= damage;
        Debug.Log($"플레이어 HP: {currentHp}");

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            currentState = PlayerState.Damaged;
            animator.SetTrigger("Hit");
        }
    }

    public void Wield(ActionType actionType)
    {
        if (!isGrounded) return;

        currentState = PlayerState.Wield;

        // 공격 채광 동일 모션
        animator.SetTrigger("Wield");

        // 분기점
        // if (actionType == ActionType.Attack) ...
    }

    public void Die()
    {
        if (currentState == PlayerState.Die) return;

        currentState = PlayerState.Die;
        animator.SetTrigger("Die");

        rb.isKinematic = true;
        this.enabled = false;

        OnPlayerDied?.Invoke();
    }


    public void Wait()
    {
        // 이미 상태가 변했으면 패스
        if (currentState == PlayerState.Die) return;

        currentState = PlayerState.Idle; // 상태는 Idle로 두거나 Victory 상태를 만들어도 됨

        // 물리 엔진 정지 (미끄러짐 방지)
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;

        // 애니메이션은 Idle로 두거나 승리 포즈가 있다면 재생
        // animator.SetTrigger("Victory"); 

        // 조작을 막기 위해 상태를 별도로 정의하거나, Update에서 플래그 체크 필요
        // 여기서는 간단하게 currentState를 활용하기 위해 Faint처럼 조작 불가 상태를 하나 만들면 좋지만,
        // 일단은 GameManager에서 입력을 막거나, 아래처럼 bool 변수를 하나 써도 됩니다.
        this.enabled = false; // 스크립트 자체를 꺼버려서 Update(조작)를 막는 가장 쉬운 방법
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