public class ReachPointObjectiveHandler : QuestObjectiveHandler
{
    public ReachPointObjectiveHandler(Quest quest, int index) : base(quest, index) { }

    public override void OnStart()
    {
        GameEvents.OnPointArrived += HandlePointArrived;
    }

    public override void OnStop()
    {
        GameEvents.OnPointArrived -= HandlePointArrived;
    }

    private void HandlePointArrived(PointID targetPointID)
    {
        if (targetPointID == objectiveData.pointID && !IsComplete())
        {
            quest.currentAmounts[objectiveIndex]++;
            GameEvents.OnQuestProgressUpdated?.Invoke();
        }
    }

    public override void EvaluateProgress() { }
}