using UnityEngine;
using System;

public class PlayerCondition : MonoBehaviour
{
    //이벤트
    public event Action<float, float> OnHpChanged;      // HP 변경
    public event Action<float, float> OnStaminaChanged; // 스테미나 변경
    public event Action OnTakeDamage;            // 피격
    public event Action OnDie;                   // 사망

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

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();

        if (GameManager.Instance != null)
        {
            LoadStatusFromManager();
        }

        this.maxHp = playerData.maxHp;
        this.maxStamina = playerData.maxStamina;
    }

    void LoadStatusFromManager()
    {
        GameData data = GameManager.Instance.LoadSceneSaveData();

        // 저장된 데이터 불러오기
        currentHp = data.currentHp;
        currentStamina = data.currentStamina;

        OnHpChanged?.Invoke(currentHp, maxHp);
        OnStaminaChanged?.Invoke(1, 1); // 꽉 채워서 (버그방지)

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
}