using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Components")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    private Queue<string> sentences = new Queue<string>();

    private Action onDialogueEnded;
    private bool isDialogueActive = false;
    private bool shouldAutoClose = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialoguePanel.SetActive(false); 
    }

    private void Update()
    {
        // 대화 중일 때 마우스 클릭하면 다음 대사
        if (isDialogueActive && Input.GetMouseButtonDown(0))
        {
            DisplayNextSentence();
        }
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

        dialoguePanel.SetActive(true);
        nameText.text = speakerName;

        sentences.Clear();
        foreach (string sentence in data.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
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

        string sentence = sentences.Dequeue();
        dialogueText.text = sentence;

        // [TODO: 타이핑 효과나 선택지 로직
        // StopAllCoroutines();
        // StartCoroutine(TypeSentence(sentence));
    }

    public void EndDialogue()
    {
        if (!isDialogueActive) return;

        isDialogueActive = false;
        dialoguePanel.SetActive(false);

        onDialogueEnded?.Invoke();

        InputControlManager.Instance.UnlockInput();
    }
}