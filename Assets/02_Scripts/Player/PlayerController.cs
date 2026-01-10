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
    Die,
    Wait
}

public class PlayerController : MonoBehaviour
{
    public event Action OnPlayerDied;

    [Header("References")]

    public PlayerData playerData;
    public PlayerCondition playerCondition;

    public Transform cameraTransform;
    public Transform characterModel;

    public Animator animator;
    private Rigidbody rb;

    public GameObject interactionBox;

    [Header("Settings")]

    public PlayerState currentState = PlayerState.Idle;

    private Vector3 moveInput;
    private bool isInteracting;
    private bool isRunning;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        playerCondition = GetComponent<PlayerCondition>();

        cameraTransform = Camera.main.transform;
        animator = GetComponentInChildren<Animator>();

        playerCondition.OnDie += HandleDie;
        playerCondition.OnTakeDamage += HandleHit;
        playerCondition.OnRevive += HandleRevive;

        InputControlManager.Instance.OnInputStateChanged += HandleInputStateChange;

        Initialize();
    }
    public void Initialize()
    {
        currentState = PlayerState.Idle;
        isGrounded = true;
        isInteracting = false;
        isRunning = false;

        rb.velocity = Vector3.zero;
        rb.isKinematic = false;
        this.enabled = true;

        animator.Rebind();
        animator.Update(0f);

        Debug.Log("플레이어 초기화 완료");
    }

    void OnDestroy()
    {
        playerCondition.OnDie -= HandleDie;
        playerCondition.OnTakeDamage -= HandleHit;
        playerCondition.OnRevive -= HandleRevive;

        InputControlManager.Instance.OnInputStateChanged -= HandleInputStateChange;
    }

    void Update()
    {
        if (currentState == PlayerState.Die || currentState == PlayerState.Wait) return;

        HandleInput();
        UpdateStateAndAnimation();
    }

    void FixedUpdate()
    {
        if (currentState == PlayerState.Die || currentState == PlayerState.Wait) return;

        HandleMove();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            //if (collision.contacts[0].normal.y > 0.5f) { }//가파른 벽인 경우(현재 없음)
            isGrounded = true;
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
        if (currentState == PlayerState.Wait)
        {
            moveInput = Vector3.zero; 
            isRunning = false;
            return;
        }

        // 입력
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0; 
        camRight.y = 0;

        moveInput = (camForward.normalized * v + camRight.normalized * h).normalized;

        // 달리기
        isRunning = Input.GetKey(KeyCode.LeftShift) && moveInput != Vector3.zero;

        // 점프
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            HandleJump();
        }

        // 회전 (액션중, 이동중)
        isInteracting = Input.GetMouseButton(0) || Input.GetMouseButton(1);

        if (isInteracting)
        {
            LookAtMouse(playerData.actionRotationSpeed);
        }
        else if (moveInput != Vector3.zero)
        {
            LookAtMoveDirection(playerData.rotationSpeed);
        }
    }

    void UpdateStateAndAnimation()
    {
        if (animator == null) return;

        if (currentState == PlayerState.Wait)
        {
            animator.SetFloat("InputX", 0);
            animator.SetFloat("InputY", 0);
            animator.SetBool("IsRunning", false);
            return;
        }

        if (!isGrounded) currentState = PlayerState.Jump;
        else if (moveInput != Vector3.zero) currentState = PlayerState.Move;
        else currentState = PlayerState.Idle;

        // 블렌드 트리 애니메이션

        //월드 기준 이동 방향을 캐릭터 기준으로 변환
        Vector3 localVelocity = characterModel.InverseTransformDirection(moveInput);

        // 걷기=0.5, 달리기=1.0
        float animSpeed = 0f;

        if (moveInput != Vector3.zero) // 이동일때
        {
            if (isRunning) //과적아닐때
                animSpeed = 1.0f;
            else
                animSpeed = 0.5f;
        }

        animator.SetFloat("InputX", localVelocity.x * animSpeed, 0.1f, Time.deltaTime);
        animator.SetFloat("InputY", localVelocity.z * animSpeed, 0.1f, Time.deltaTime);

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsRunning", isRunning);
    }

    public void HandleWield(ActionType actionType)
    {
        if (!isGrounded) return;

        currentState = PlayerState.Wield;

        // 공격 채광 동일 모션
        animator.SetTrigger("Wield");

        // 분기점
        // if (actionType == ActionType.Attack) ...
    }

    void HandleMove()
    {
        if (moveInput == Vector3.zero) return;

        bool tryRun = Input.GetKey(KeyCode.LeftShift) && playerCondition.CanRun();

        if (tryRun) HandleRun();
        else HandleWalk();
    }

    void HandleRun()
    {
        // 소모량
        float cost = playerCondition.runCostPerSec * Time.fixedDeltaTime;

        if (playerCondition.UseStamina(cost))
        {
            isRunning = true;
            float speed = playerData.runSpeed;
            rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
        }
        else
        {
            HandleWalk();
        }
    }

    void HandleWalk()
    {
        isRunning = false;
        float speed = playerCondition.GetWalkSpeed(playerData);
        rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
    }    

    void HandleJump()
    {
        rb.AddForce(Vector3.up * playerData.jumpForce, ForceMode.Impulse);
        isGrounded = false;
        currentState = PlayerState.Jump;
        animator.SetTrigger("Jump");
    }

    void HandleHit()
    {
        if (currentState == PlayerState.Die) return;

        currentState = PlayerState.Damaged;
        animator.SetTrigger("Hit");
    }

    public void HandleDie()
    {
        if (currentState == PlayerState.Die) return;

        currentState = PlayerState.Die;
        animator.SetTrigger("Die");

        rb.isKinematic = true;
        this.enabled = false;

        OnPlayerDied?.Invoke();
    }
    private void HandleRevive()
    {
        if (currentState == PlayerState.Die)
        {
            Initialize();

            animator.SetBool("IsDead", false);
            animator.Play("Idle");

            Debug.Log("PlayerController: 부활 신호 수신 -> 상태 리셋됨");
        }
    }

    private void HandleInputStateChange(bool canInput)
    {
        if (!canInput)
        {
            Wait(); 
        }
        else
        {
            WaitDone();
        }
    }

    private void Wait()
    {
        if (currentState == PlayerState.Die) return;

        currentState = PlayerState.Wait;

        rb.velocity = Vector3.zero;

        if (animator != null)
        {
            animator.SetFloat("InputX", 0);
            animator.SetFloat("InputY", 0);
            animator.SetBool("IsRunning", false);
        }

    }

    private void WaitDone()
    {
        if (currentState == PlayerState.Wait)
        {
            currentState = PlayerState.Idle;
            rb.isKinematic = false;
        }

    }

    #region 캐릭터 방향
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
    #endregion 

}