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

    private int hpLevel;
    private int staminaLevel;
    private float maxHp;
    private float maxStamina;
    private bool isExhausted = false;
   

    public float currentHp { get; private set; }
    public float currentStamina { get; private set; }
    public float MaxHp => maxHp;
    public float MaxStamina => maxStamina;
    public bool IsDead => currentHp <= 0;

    [Header("Settings")]
    public float staminaRecovery = 15f;
    public float recoveryDelay = 1.0f;
    public float runCostPerSec = 10f;
    public float jumpCost = 20f;
    private float lastStaminaUseTime;
    public float staminaRecoveryThreshold = 0.2f;


    private void OnEnable()
    {
        if(GameManager.Instance != null)
            GameManager.Instance.OnSleep += FullRecovery; 
    }
    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnSleep -= FullRecovery;
    }

    private void Awake()
    {
        if (playerData != null)
        {
            // 데이터 로드 전 잠깐이라도 풀피로 설정해둠
            // 이렇게 해야 IsDead가 false가 되어 PlayerController가 Start에서 죽는 처리를 안함
            currentHp = playerData.GetMaxHpValue(0);
            currentStamina = playerData.GetMaxStaminaValue(0);
        }
    }

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        LoadStatusFromManager();
        
    }

    public void LoadStatusFromManager()
    {
        var data = GameSaveManager.Instance.LoadPlayerCondition();

        // 저장된 데이터 불러오기
        this.hpLevel = data.hpLevel;
        this.staminaLevel = data.staminaLevel;

        RefreshMaxStats();

        this.currentHp = data.hp;
        this.currentStamina = data.stamina;

        OnHpChanged?.Invoke(currentHp, maxHp);
        OnStaminaChanged?.Invoke(maxStamina, maxStamina); // 꽉 채워서 (버그방지)


        Debug.Log($"상태 로드 완료: HP {currentHp}, Stamina {currentStamina}");
    }

    public void RefreshMaxStats()
    {
        if (playerData != null)
        {
            maxHp = playerData.GetMaxHpValue(hpLevel);
            maxStamina = playerData.GetMaxStaminaValue(staminaLevel);
        }
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
            if (isExhausted && currentStamina > (maxStamina * staminaRecoveryThreshold))
            {
                isExhausted = false;
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

        if (playerData.hitSound != null) SoundManager.Instance.PlaySFX(playerData.hitSound, transform.position);

        currentHp -= amount;

        // UI 갱신
        OnHpChanged?.Invoke(currentHp, maxHp);

        if (currentHp <= 0)
        {
            if(playerData.deathSound != null) SoundManager.Instance.PlaySFX(playerData.deathSound, transform.position);    
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

    public void FullRecovery()
    {
        if (IsDead) return;

        currentHp = maxHp;
        currentStamina = maxStamina;

        OnHpChanged?.Invoke(currentHp, maxHp);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
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
        else
        {
            if (!isExhausted && currentStamina <= 0)
            {
                PlayExhaustedSound();
            }
            return false;
        }
    }
    private void PlayExhaustedSound()
    {
        if (playerData.exhaustedSound != null && SoundManager.Instance != null)
        {
            isExhausted = true;
            SoundManager.Instance.PlaySFX(playerData.exhaustedSound, transform.position);

            // 일정 시간 후 또는 스테미나가 일정량 회복된 후 다시 재생 가능하도록
        }
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
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SavePlayerCondition(
                currentHp,
                currentStamina,
                hpLevel,
                staminaLevel
            );
        }
    }
}