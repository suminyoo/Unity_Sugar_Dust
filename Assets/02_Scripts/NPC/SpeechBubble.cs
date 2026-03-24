using UnityEngine;
using TMPro;
using System.Collections;

public class SpeechBubble : MonoBehaviour
{
    public GameObject bubbleCanvas;
    public TextMeshProUGUI bubbleText;
    public TextMeshProUGUI nameText;

    private void Start()
    {
        bubbleCanvas.SetActive(false);
        SetupNameTag();
    }
    private void SetupNameTag()
    {
        if (nameText == null) return;

        NPCController controller = GetComponentInParent<NPCController>();

        if (controller == null)
        {
            nameText.gameObject.SetActive(false);
            return;
        }

        string npcName = controller.GetNpcName();

        if (string.IsNullOrEmpty(npcName) || npcName == "???")
        {
            nameText.gameObject.SetActive(false);
        }
        else
        {
            nameText.text = npcName;
            nameText.gameObject.SetActive(true);
        }
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