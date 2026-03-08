using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    #region Variables & References

    [Header("References")]
    public PlayerCondition playerCondition;

    [Header("HP")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    [Header("Stemina")]
    public Slider staminaSlider;
    public float barAnimationDuration = 0.2f; // 스테미나 차기 전 딜레이시간

    public float staminaLerpSpeed = 15f;
    public float potionLerpSpeed = 50f;
    private float currentLerpSpeed;
    private float targetStaminaRatio = 1f;

    private float hideDelay = 1f;
    private Coroutine hideStaminaCoroutine;

    [Header("Money")]
    public TextMeshProUGUI moneyText;

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        currentLerpSpeed = staminaLerpSpeed;

        var playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerCondition = playerObj.GetComponent<PlayerCondition>();

            // 이벤트를 구독
            playerCondition.OnHpChanged += UpdateHpUI;
            playerCondition.OnStaminaChanged += UpdateStaminaUI;

            // 구독 직후 UI 수동 초기화
            UpdateHpUI(playerCondition.currentHp, playerCondition.MaxHp);
            UpdateStaminaUI(playerCondition.currentStamina, playerCondition.MaxStamina);
        }

        PlayerAssetsManager.Instance.OnMoneyChanged += UpdateMoneyUI;
        UpdateMoneyUI(PlayerAssetsManager.Instance.CurrentMoney);

    }

    void OnDestroy()
    {
        playerCondition.OnHpChanged -= UpdateHpUI;
        playerCondition.OnStaminaChanged -= UpdateStaminaUI;
        PlayerAssetsManager.Instance.OnMoneyChanged -= UpdateMoneyUI;
    }

    #endregion


    #region HP & Stamina 
    private void UpdateHpUI(float curHp, float maxHp)
    {
        //Debug.Log($"[PlayerHUD] HP UI 업데이트: {curHp} / {maxHp}");
        hpSlider.value = curHp / maxHp;
        hpText.text = $"{(int)curHp} / {(int)maxHp}";
    }

    private void UpdateStaminaUI(float curStem, float maxStem)
    {
        float ratio = curStem / maxStem;

        //물약회복
        if (ratio - targetStaminaRatio > 0.05f)
        {
            currentLerpSpeed = potionLerpSpeed;
        }

        targetStaminaRatio = ratio;

        // 100%면 숨김 대기 시작
        if (ratio >= 1f)
        {
            if (hideStaminaCoroutine == null)
                hideStaminaCoroutine = StartCoroutine(HideStaminaAfterDelay());
            return;
        }

        // 값이 줄어들면 숨김 취소하고 즉시 표기
        if (hideStaminaCoroutine != null)
        {
            StopCoroutine(hideStaminaCoroutine);
            hideStaminaCoroutine = null;
        }

        staminaSlider.gameObject.SetActive(true);
    }

    private IEnumerator HideStaminaAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);

        staminaSlider.gameObject.SetActive(false);
        hideStaminaCoroutine = null;
    }
    private void Update()
    {
        if (staminaSlider.gameObject.activeSelf)
        {
            staminaSlider.value = Mathf.Lerp(staminaSlider.value, targetStaminaRatio, Time.deltaTime * currentLerpSpeed);

            if (currentLerpSpeed == potionLerpSpeed && Mathf.Abs(staminaSlider.value - targetStaminaRatio) < 0.01f)
            {
                currentLerpSpeed = staminaLerpSpeed;
            }
        }
    }

    #endregion

    #region Money 

    private void UpdateMoneyUI(int currentGold)
    {
        if (moneyText != null)
        {
            moneyText.text = $"{currentGold:N0} {CustomerPaymentSystem.CURRENCY_SYMBOL}";
        }
    }

    #endregion
}