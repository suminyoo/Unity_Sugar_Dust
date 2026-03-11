public class DestinationObjectiveHandler : QuestObjectiveHandler
{
    public DestinationObjectiveHandler(Quest quest, int index) : base(quest, index) { }

    public override void OnStart()
    {
        GameEvents.OnPointArrived += HandlePointArrived;
    }

    public override void OnStop()
    {
        GameEvents.OnPointArrived -= HandlePointArrived;
    }

    private void HandlePointArrived(DestinationID targetPointID)
    {
        if (targetPointID == objectiveData.destinationID && !IsComplete())
        {
            quest.currentAmounts[objectiveIndex]++;
            GameEvents.OnQuestProgressUpdated?.Invoke();
        }
    }

    public override void EvaluateProgress() { }
}