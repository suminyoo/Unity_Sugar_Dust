using UnityEngine;
using TMPro;
using System.Collections;

public class SpeechBubble : MonoBehaviour
{
    public GameObject bubbleCanvas; // 캔버스 오브젝트
    public TextMeshProUGUI bubbleText;

    private void Start()
    {
        bubbleCanvas.SetActive(false); // 평소엔 숨김
    }

    public void ShowBubble(string text, float duration = 2.0f)
    {
        StopAllCoroutines(); // 기존 말풍선 코루틴 취소
        StartCoroutine(BubbleRoutine(text, duration));
    }

    private IEnumerator BubbleRoutine(string text, float duration)
    {
        bubbleText.text = text;
        bubbleCanvas.SetActive(true);

        yield return new WaitForSeconds(duration);

        bubbleCanvas.SetActive(false);
    }
}