using System.Collections.Generic;

//인게임에서 사용할 퀘스트 진행상황 저장 클래스
[System.Serializable]
public class Quest
{
    public QuestData data;
    public int[] currentAmounts;
    public bool isRewardClaimed = false;

    // 목표별 담당자
    private List<QuestObjectiveHandler> handlers = new List<QuestObjectiveHandler>();

    public Quest(QuestData data)
    {
        this.data = data;
        currentAmounts = new int[data.objectives.Count];

        // 목표 타입에 맞춰서 목표별 담당자에게 
        for (int i = 0; i < data.objectives.Count; i++)
        {
            var obj = data.objectives[i];
            if (obj.type == QuestType.Collect)
            {
                handlers.Add(new CollectObjectiveHandler(this, i));

            }
            else if (obj.type == QuestType.Hunt)
            {
                handlers.Add(new HuntObjectiveHandler(this, i));

            }
            // 새로운 퀘스트(장소도달?) 생기면 여기에 elseif로 늘려가기
        }
    }

    public void StartQuest()
    {
        foreach (var handler in handlers) handler.OnStart();
    }

    public void StopQuest()
    {
        foreach (var handler in handlers) handler.OnStop();
    }

    //퀘스트매니저의 검사
    public void EvaluateAll()
    {
        foreach (var handler in handlers) handler.EvaluateProgress();
    }

    public bool IsAllObjectivesComplete()
    {
        EvaluateAll();
        foreach (var handler in handlers)
        {
            if (!handler.IsComplete()) return false;
        }
        return true;
    }

    //public string GetObjectiveText(int index)
    //{
    //    return handlers[index].GetProgressText();
    //}
}