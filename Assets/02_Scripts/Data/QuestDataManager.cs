using UnityEngine;
using System.Collections.Generic;

public class QuestDataManager : MonoBehaviour
{
    public static QuestDataManager Instance;

    private Dictionary<string, QuestData> questDatabase = new Dictionary<string, QuestData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadAllQuests();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAllQuests()
    {
        QuestData[] quests = Resources.LoadAll<QuestData>("Quest");

        foreach (var quest in quests)
        {
            if (string.IsNullOrEmpty(quest.questID))
            {
                Debug.LogWarning($"[경고] {quest.name}의 questID가 비어있습니다");
                continue;
            }

            // 등록
            if (!questDatabase.ContainsKey(quest.questID))
            {
                questDatabase.Add(quest.questID, quest);
            }
        }
        //Debug.Log($"총 {questDatabase.Count}개의 퀘스트 데이터를 성공적으로 불러왔습니다.");
    }

    public QuestData GetQuestByID(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        if (questDatabase.TryGetValue(id, out QuestData quest))
        {
            return quest;
        }

        Debug.LogError($"'{id}'를 가진 퀘스트를 찾을 수 없습니다!");
        return null;
    }
}