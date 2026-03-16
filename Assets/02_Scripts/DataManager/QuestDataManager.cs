using UnityEngine;
using System.Collections.Generic;
public enum QuestID //순서나 이름 수정 불가
{
    None = 0,
    TestQuest_1 = 1,
    TestQuest_2 = 2,
    TestQuest_3 = 3,
    TestQuest_4 = 4,


    Tuto_01 = 10,
    Tuto_02 = 11,
    Tuto_03 = 12,
    Tuto_04 = 13,
    Tuto_05 = 14,
    Tuto_06 = 15,
    Tuto_07 = 16,
    Tuto_08 = 17,
    Tuto_09 = 18,
    Tuto_10 = 19,
    Tuto_11 = 20,


}

public class QuestDataManager : MonoBehaviour
{
    public static QuestDataManager Instance;

    private Dictionary<QuestID, QuestData> questDatabase = new Dictionary<QuestID, QuestData>();

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
            if (quest.questID == QuestID.None) continue;

            if (!questDatabase.ContainsKey(quest.questID))
            {
                questDatabase.Add(quest.questID, quest);
            }
        }
    }

    public QuestData GetQuestByID(QuestID id)
    {
        if (id == QuestID.None) return null;
        return questDatabase.TryGetValue(id, out QuestData data) ? data : null;
    }

    //세이브 데이터용
    public QuestData GetQuestByID(string idString)
    {
        if (System.Enum.TryParse(idString, out QuestID id))
        {
            return GetQuestByID(id);
        }
        return null;
    }

}