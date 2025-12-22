using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 15f;

    [Header("References")]
    public Transform cameraTransform;
    public Transform characterModel; // 자식인 Model을 여기에 연결하세요.

    private Rigidbody rb;
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // 부모의 회전은 고정 (물리 충돌로 돌아가지 않게)
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // 1. 입력 및 방향 계산
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0; camRight.y = 0;

        moveInput = (camForward.normalized * v + camRight.normalized * h).normalized;

        // 2. 자식 모델만 회전 (부모는 회전하지 않음)
        if (moveInput != Vector3.zero && characterModel != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            characterModel.rotation = Quaternion.Slerp(characterModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        // 3. 부모(Cube) 이동
        if (moveInput != Vector3.zero)
        {
            rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        }
    }
}