public abstract class QuestObjectiveHandler
{
    protected Quest quest;
    protected int objectiveIndex;
    protected QuestObjective objectiveData;

    public QuestObjectiveHandler(Quest quest, int objectiveIndex)
    {
        this.quest = quest;
        this.objectiveIndex = objectiveIndex;
        this.objectiveData = quest.data.objectives[objectiveIndex];
    }

    // 퀘스트를 받을 때
    public virtual void OnStart() { }

    // 퀘스트 완료 시
    public virtual void OnStop() { }

    // 상태 검사 (Q창이나 NPC퀘창 등 검사가 필요한 시점에서 호출)
    public abstract void EvaluateProgress();

    //public abstract string GetProgressText();

    // 완료여부
    public bool IsComplete()
    {
        return quest.currentAmounts[objectiveIndex] >= objectiveData.requiredAmount;
    }
}