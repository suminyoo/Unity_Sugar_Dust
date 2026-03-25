using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ItemNotificationSlot : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI itemText;

    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.4f;
    public float displayTime = 2.0f;
    public Vector2 startOffset = new Vector2(-50f, 0f);

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalPosition;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }
    public void SetData(ItemData data, int amount)
    {
        itemIcon.sprite = data.icon;
        itemText.text = $"{data.GetItemName()} + {amount}";

        StartCoroutine(AnimateNotification());
    }

    private IEnumerator AnimateNotification()
    {
        canvasGroup.alpha = 0f;

        yield return new WaitForEndOfFrame();

        float targetX = rectTransform.anchoredPosition.x;
        float startX = targetX + startOffset.x;

        float time = 0f;
        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeInDuration;
            float easeT = Mathf.SmoothStep(0f, 1f, t);

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, easeT);

            float currentY = rectTransform.anchoredPosition.y;
            rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, easeT), currentY);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = new Vector2(targetX, rectTransform.anchoredPosition.y);

        yield return new WaitForSeconds(displayTime);

        time = 0f;

        Vector2 exitStartPos = rectTransform.anchoredPosition;
        Vector2 exitTargetPos = exitStartPos + new Vector2(0f, 30f);

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeOutDuration;
            float easeT = Mathf.SmoothStep(0f, 1f, t);

            canvasGroup.alpha = Mathf.Lerp(1f, 0f, easeT);
            rectTransform.anchoredPosition = Vector2.Lerp(exitStartPos, exitTargetPos, easeT);

            yield return null;
        }

        if (ItemNotificationManager.Instance != null)
        {
            ItemNotificationManager.Instance.RemoveFromList(gameObject);
        }
        Destroy(gameObject);
    }
}