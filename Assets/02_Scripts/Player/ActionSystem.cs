using UnityEngine;
using UnityEngine.EventSystems;

public interface IMineable { void OnMine(float power, bool isCritical); }
public interface IDamageable { void TakeDamage(float damage, bool isCritical); }

public class ActionSystem : MonoBehaviour
{
    public enum ActionType
    {
        Attack,
        Mine
    }

    [Header("Settings")]
    public Transform firePoint;
    public Transform handHolder;    // 무기가 생성될 손 위치 (부모)
    public LayerMask actionLayer;
    public PlayerAttackZone attackZone;

    [Header("Tool Data")]
    public ToolData currentSwordData;   // 현재 장착된 검 데이터
    public ToolData currentPickaxeData; // 현재 장착된 곡괭이 데이터
    private ToolData activeToolData;    // 현재 행동에 사용될 데이터 캐싱
    private float currentCooldownTimer = 0f;

    [Header("System Info")]
    private PlayerController playerController;
    private ActionType currentActionType; //현재행동

    // 생성된 무기 모델을 관리하기 위한 변수
    private GameObject instantiatedWeaponModel;
    private ActionType lastEquippedType; // 마지막으로 든 무기 타입을 기억

    private bool isActionLocked = false; //입력 차단
    private RaycastHit currentHit; // 레이캐스트 결과를 저장할 변수

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        EquipTool(ActionType.Attack);
        InputControlManager.Instance.OnInputStateChanged += (canInput) =>
        {
            isActionLocked = !canInput;
        };
    }
    // 애니메이션 이벤트: EnableAttackHitBox 일 때 호출
    public void OnAttackStart()
    {
        if (activeToolData == null || attackZone == null) return;

        // 데미지 계산
        bool isCritical = UnityEngine.Random.value < activeToolData.criticalChance;
        float finalDamage = activeToolData.power;
        if (isCritical) finalDamage *= activeToolData.criticalMultiplier;

        // 히트박스 켜기! (데미지 정보 전달)
        attackZone.EnableZone(finalDamage, isCritical, currentActionType);
    }

    // 애니메이션 이벤트: DisableAttackHitBox 일 때 호출
    public void OnAttackEnd()
    {
        if (attackZone != null)
        {
            attackZone.DisableZone();
        }
    }

    void Update()
    {
        // 레이로 물체감지
        UpdateRaycast();

        HandleActionInput();
        
    }
    // 도구 외형 및 데이터 교체 (중요!)
    // 상점에서 업그레이드 하거나, 공격/채광 모드 전환 시 호출
    public void EquipTool(ActionType actionType)
    {
        if (instantiatedWeaponModel != null) Destroy(instantiatedWeaponModel);

        if (actionType == ActionType.Attack)
            activeToolData = currentSwordData;
        else
            activeToolData = currentPickaxeData;


        // 생성
        if (activeToolData != null && activeToolData.toolPrefab != null && handHolder != null)
        {
            //if(activeToolData.equipSound.clip != null) SoundManager.Instance.PlaySFX(activeToolData.equipSound, transform.position);

            instantiatedWeaponModel = Instantiate(activeToolData.toolPrefab, handHolder);
        }

        lastEquippedType = actionType;
    }

    // 레이로 타겟 감지 하고 텍스트 변경요청
    void UpdateRaycast()
    {
        if (firePoint == null) return;
        float range = activeToolData != null ? activeToolData.range : 2f;

        Ray ray = new Ray(firePoint.position, firePoint.forward);

        // 레이어 마스크
        if (Physics.Raycast(ray, out currentHit, range, actionLayer))
        {

            // 적 IDamageable
            if (currentHit.collider.GetComponentInParent<IDamageable>() != null)
            {
                PromptUIManager.Instance.SetActionPrompt(LocalizationHelper.Main("PROMPT_ACTION_ATTACK"));
            }
            // 광물 IMineable
            else if (currentHit.collider.GetComponentInParent<IMineable>() != null)
            {
                PromptUIManager.Instance.SetActionPrompt(LocalizationHelper.Main("PROMPT_ACTION_MINE"));
            }
            else
            {
                PromptUIManager.Instance.ClearActionPrompt();
            }
        }
        else
        {
            PromptUIManager.Instance.ClearActionPrompt();
        }
    }

    void HandleActionInput()
    {
        if (isActionLocked) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // 쿨타임
        if (currentCooldownTimer > 0) currentCooldownTimer -= Time.deltaTime;

        // 좌클릭: 공격
        if (Input.GetMouseButton(0) && currentCooldownTimer <= 0f)
        {
            if (currentActionType != ActionType.Attack) EquipTool(ActionType.Attack);

            StartAction(ActionType.Attack);
        }

        // 우클릭: 채광
        if (Input.GetMouseButton(1) && currentCooldownTimer <= 0f)
        {
            if (currentActionType != ActionType.Mine) EquipTool(ActionType.Mine);

            StartAction(ActionType.Mine);
        }
    }

    void StartAction(ActionType actionType)
    {
        currentActionType = actionType;

        // 데이터에서 쿨타임 가져오기
        currentCooldownTimer = activeToolData != null ? activeToolData.cooldown : 1f;

        playerController.HandleWield(actionType); // 애니메이션 재생

        if (activeToolData != null && activeToolData.actionSound.clip != null)
        {
            SoundManager.Instance.PlaySFX(activeToolData.actionSound, transform.position);
        }
    }

    // 상점에서 무기 업그레이드 시 호출용
    public void UpgradeTool(ToolData newToolData, ActionType type)
    {
        if (type == ActionType.Attack) currentSwordData = newToolData;
        else currentPickaxeData = newToolData;

        if (lastEquippedType == type)
        {
            EquipTool(type);
        }
    }
}