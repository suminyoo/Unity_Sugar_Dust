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

public class PossessMoneyObjectiveHandler : QuestObjectiveHandler
{
    public PossessMoneyObjectiveHandler(Quest quest, int index) : base(quest, index) { }

    public override void EvaluateProgress()
    {
        if (GameEvents.RequestPlayerMoney != null)
        {
            quest.currentAmounts[objectiveIndex] = GameEvents.RequestPlayerMoney.Invoke();
        }
    }
}

public class EarnMoneyObjectiveHandler : QuestObjectiveHandler
{
    public EarnMoneyObjectiveHandler(Quest quest, int index) : base(quest, index) { }

    public override void OnStart() => GameEvents.OnRevenueEarned += HandleRevenue;
    public override void OnStop() => GameEvents.OnRevenueEarned -= HandleRevenue;

    private void HandleRevenue(int amount)
    {
        if (amount > 0 && !IsComplete())
        {
            quest.currentAmounts[objectiveIndex] += amount;

            if (quest.currentAmounts[objectiveIndex] > objectiveData.requiredAmount)
                quest.currentAmounts[objectiveIndex] = objectiveData.requiredAmount;

            GameEvents.OnQuestProgressUpdated?.Invoke();
        }
    }
    public override void EvaluateProgress() { } 
}