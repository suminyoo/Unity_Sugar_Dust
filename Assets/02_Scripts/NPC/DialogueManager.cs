using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro 사용 권장
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Components")]
    public GameObject dialoguePanel; // 대화창 전체 패널
    public TextMeshProUGUI nameText; // 화자 이름 텍스트
    public TextMeshProUGUI dialogueText; // 대사 내용 텍스트

    private Queue<string> sentences = new Queue<string>();
    private Action onDialogueEnded; // 대화 끝났을 때 NPC에게 알려줄 신호
    private bool isDialogueActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialoguePanel.SetActive(false); // 시작할 땐 꺼두기
    }

    private void Update()
    {
        // 대화 중일 때 마우스 클릭하면 다음 대사로
        if (isDialogueActive && Input.GetMouseButtonDown(0))
        {
            DisplayNextSentence();
        }
    }

    // NPCBrain에서 이 함수를 호출해서 대화를 시작함
    public void StartDialogue(DialogueData data, Action callback)
    {
        if (data == null) return;

        isDialogueActive = true;
        onDialogueEnded = callback; // 끝날 때 실행할 함수 저장

        dialoguePanel.SetActive(true);
        nameText.text = data.speakerName;

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
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        dialogueText.text = sentence;

        // [추후 구현: 타이핑 효과나 선택지 로직이 여기 들어감]
        // StopAllCoroutines();
        // StartCoroutine(TypeSentence(sentence));
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);

        // NPC에게 "대화 끝났어!"라고 알림
        onDialogueEnded?.Invoke();
    }
}