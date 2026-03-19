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