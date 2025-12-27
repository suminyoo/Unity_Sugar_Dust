using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{
    [Header("References")]
    public PlayerCondition playerCondition; 

    public Slider staminaSlider;

    public float barAnimationDuration = 0.2f; // 변화하는 데 걸리는 시간

    private Coroutine staminaCoroutine;

    void Start()
    {
        playerCondition = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCondition>();
        
        playerCondition.OnStaminaChanged += UpdateStaminaUI;
        // playerCondition.OnHpChanged += UpdateHpUI; 
    }

    void OnDestroy()
    { 
        playerCondition.OnStaminaChanged -= UpdateStaminaUI;
    }

    private void UpdateStaminaUI(float ratio)
    {
        StopCoroutine(staminaCoroutine);
        staminaCoroutine = StartCoroutine(UpdateBarSmoothly(staminaSlider, ratio));
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
}