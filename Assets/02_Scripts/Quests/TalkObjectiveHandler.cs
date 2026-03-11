public class TalkObjectiveHandler : QuestObjectiveHandler
{
    public TalkObjectiveHandler(Quest quest, int index) : base(quest, index) { }

    public override void OnStart()
    {
        GameEvents.OnNPCTalked += HandleNPCTalked;
    }

    public override void OnStop()
    {
        GameEvents.OnNPCTalked -= HandleNPCTalked;
    }

    private void HandleNPCTalked(NPCID targetNpcID)
    {
        if (targetNpcID == objectiveData.npcID && !IsComplete())
        {
            quest.currentAmounts[objectiveIndex]++;
            GameEvents.OnQuestProgressUpdated?.Invoke();
        }
    }

    public override void EvaluateProgress() { }
}