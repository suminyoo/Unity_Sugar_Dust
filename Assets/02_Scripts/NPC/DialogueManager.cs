using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public AlienVoicePlayer alienVoice;

    [Header("UI Components")]
    public GameObject dialogueRootPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    public GameObject dialogueUI;

    private Queue<string> sentences = new Queue<string>();

    private Action onDialogueEnded;
    public bool isDialogueActive = false;
    private bool shouldAutoClose = true;

    [Header("Typing Effects")]
    private bool isTyping = false;
    private string currentSentence = "";
    public float typeSpeed = 0.08f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialogueRootPanel.SetActive(false); 
    }

    // NPCBrain에서 이 함수를 호출해서 대화시작
    public void StartDialogue(DialogueData data, string speakerName, Action callback, bool autoClose = true)
    {
        if (data == null) return;

        InputControlManager.Instance.LockInput();

        Cursor.visible = true;
        isDialogueActive = true;
        onDialogueEnded = callback; // 끝날 때 실행할 함수 저장
        shouldAutoClose = autoClose;

        dialogueRootPanel.SetActive(true);
        nameText.text = speakerName;

        sentences.Clear();
        foreach (string sentence in data.GetSentences())
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }
    public void OnDialoguePanelClicked()
    {
        if (!isDialogueActive) return;

        if (isTyping)
        {
            // 타이핑 중일 때 누르면 스킵
            StopAllCoroutines();
            dialogueText.text = currentSentence;
            isTyping = false;
        }
        else
        {
            // 타이핑이 이미 다 끝났을 때 누르면 다음 문wkd
            DisplayNextSentence();
        }
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            if (shouldAutoClose)
            {
                EndDialogue();
            }
            return;
        }

        currentSentence = sentences.Dequeue();

        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
    }
    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isTyping = true;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;

            // 띄어쓰기가 아닐 때만 소리 재생
            if (letter != ' ' && alienVoice != null)
            {
                alienVoice.PlayRandomSyllable();
            }

            if (letter == '.' || letter == ',' || letter == '?' || letter == '!')
            {
                yield return new WaitForSeconds(typeSpeed * 4f);
            }
            else
            {
                yield return new WaitForSeconds(typeSpeed);
            }
        }

        isTyping = false; 
    }
    public void EndDialogue()
    {
        if (!isDialogueActive) return;

        isDialogueActive = false;
        dialogueRootPanel.SetActive(false);

        onDialogueEnded?.Invoke();

        InputControlManager.Instance.UnlockInput();
    }
}