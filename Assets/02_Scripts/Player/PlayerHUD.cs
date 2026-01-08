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
    private Coroutine staminaCoroutine;
    public float hideDelay = 0.5f;
    private Coroutine hideStaminaCoroutine;

    [Header("Money")]
    public TextMeshProUGUI moneyText;

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        playerCondition = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCondition>();

        playerCondition.OnHpChanged += UpdateHpUI;
        playerCondition.OnStaminaChanged += UpdateStaminaUI;

        PlayerAssetsManager.Instance.OnMoneyChanged += UpdateMoneyUI;

        staminaSlider.gameObject.SetActive(staminaSlider.value < 1f);

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
        hpSlider.value = curHp / maxHp;
        hpText.text = $"{(int)curHp} / {(int)maxHp}";
    }

    private void UpdateStaminaUI(float curStem, float maxStem)
    {
        float ratio = curStem / maxStem;
        // 100%면 숨김 
        if (ratio >= 1f)
        {
            if (hideStaminaCoroutine == null)
                hideStaminaCoroutine = StartCoroutine(HideStaminaAfterDelay());
            return;
        }

        // 값이 줄어들면 표기
        if (hideStaminaCoroutine != null)
        {
            StopCoroutine(hideStaminaCoroutine);
            hideStaminaCoroutine = null;
        }

        staminaSlider.gameObject.SetActive(true);

        if (staminaCoroutine != null)
            StopCoroutine(staminaCoroutine);

        staminaCoroutine = StartCoroutine(UpdateBarSmoothly(staminaSlider, ratio));
    }

    private IEnumerator HideStaminaAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);

        staminaSlider.gameObject.SetActive(false);
        hideStaminaCoroutine = null;
    }

    private IEnumerator UpdateBarSmoothly(Slider slider , float targetNormalizedValue)
    {
        float initialNormalizedValue = slider.value;
        float elapsedTime = 0f;

        while (elapsedTime < barAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / barAnimationDuration;

            slider.value = Mathf.Lerp(initialNormalizedValue, targetNormalizedValue, t);

            yield return null;
        }

        // 끝난 후 최종 값으로 정확히 설정
        slider.value = targetNormalizedValue;
    }

    #endregion

    #region Money 

    private void UpdateMoneyUI(int currentGold)
    {
        if (moneyText != null)
        {
            moneyText.text = $"{currentGold:N0} G";
        }
    }

    #endregion
}