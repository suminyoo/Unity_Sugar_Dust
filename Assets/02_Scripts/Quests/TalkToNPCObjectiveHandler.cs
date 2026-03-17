public class TalkToNPCObjectiveHandler : QuestObjectiveHandler
{
    public TalkToNPCObjectiveHandler(Quest quest, int index) : base(quest, index) { }

    public override void OnStart()
    {
        GameEvents.OnNPCTalkedFinished += HandleNPCTalked;
    }

    public override void OnStop()
    {
        GameEvents.OnNPCTalkedFinished -= HandleNPCTalked;
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