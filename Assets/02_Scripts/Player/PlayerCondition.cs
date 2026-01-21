using UnityEngine;
using System;

public class PlayerCondition : MonoBehaviour, ISaveable
{
    //이벤트
    public event Action<float, float> OnHpChanged;      // HP 변경
    public event Action<float, float> OnStaminaChanged; // 스테미나 변경
    public event Action OnTakeDamage;            // 피격
    public event Action OnDie;                   // 사망
    public event Action OnRevive;                // 부활

    [Header("References")]
    public PlayerInventory inventory;
    public PlayerData playerData;

    private float maxHp;
    private float maxStamina;

    public float currentHp { get; private set; }
    public float currentStamina { get; private set; }

    public bool IsDead => currentHp <= 0;

    [Header("Settings")]
    public float staminaRecovery = 15f;
    public float recoveryDelay = 1.0f;
    public float runCostPerSec = 10f;
    public float jumpCost = 20f;
    private float lastStaminaUseTime;

    private void Awake()
    {
        if (playerData != null)
        {
            // 데이터 로드 전 잠깐이라도 풀피로 설정해둠
            // 이렇게 해야 IsDead가 false가 되어 PlayerController가 Start에서 죽는 처리를 안함
            currentHp = playerData.maxHp;
            currentStamina = playerData.maxStamina;
        }
    }

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();


        LoadStatusFromManager();
        
    }

    void LoadStatusFromManager()
    {
        var data = GameSaveManager.Instance.LoadPlayerCondition();

        // 저장된 데이터 불러오기
        currentHp = data.hp;
        currentStamina = data.stamina;

        this.maxHp = playerData.maxHp;
        this.maxStamina = playerData.maxStamina;

        OnHpChanged?.Invoke(currentHp, maxHp);
        OnStaminaChanged?.Invoke(maxStamina, maxStamina); // 꽉 채워서 (버그방지)

        Debug.Log($"상태 로드 완료: HP {currentHp}, Stamina {currentStamina}");
    }

    void Update()
    {
        if (IsDead) return;

        // 스테미나 자연 회복
        if (Time.time - lastStaminaUseTime > recoveryDelay)
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRecovery * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
                OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            }
        }
    }
    public float GetCurrentWeightRatio()
    {
        if (inventory == null) return 0f;

        float max = inventory.maxWeight > 0 ? inventory.maxWeight : 1f;
        return inventory.currentWeight / max;
    }

    public bool CanRun()
    {
        float ratio = GetCurrentWeightRatio();

        if(ratio <= 0.8f && currentStamina > 0) 
            return true;
        else 
            return false;
    }


    public void TakeDamage(float amount = 10f)
    {
        if (IsDead) return;

        currentHp -= amount;

        // UI 갱신
        OnHpChanged?.Invoke(currentHp, maxHp);

        if (currentHp <= 0)
        {
            currentHp = 0;
            OnDie?.Invoke();
        }
        else
        {
            OnTakeDamage?.Invoke();
        }
    }
    public void Revive(float recoverAmount)
    {
        // 체력 회복
        currentHp = recoverAmount;
        if (currentHp > maxHp) currentHp = maxHp;

        currentStamina = maxStamina;

        OnRevive?.Invoke();

        OnHpChanged?.Invoke(currentHp, maxHp);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);

        Debug.Log($"플레이어 부활 완료 HP: {currentHp}");
    }
    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            lastStaminaUseTime = Time.time;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            return true;
        }
        return false;
    }
    public float GetWalkSpeed(PlayerData data)
    {
        float ratio = GetCurrentWeightRatio();

        if (ratio >= 1.0f) return data.tooHeavySpeed; // 과적
        if (ratio >= 0.8f) return data.heavySpeed;    // 무거움
        return data.walkSpeed;                        // 정상
    }

    public void SaveData()
    {
        if (GameSaveManager.Instance != null && inventory != null)
        {
            GameSaveManager.Instance.SavePlayerState(
               currentHp,
               currentStamina
           );
        }
    }
}