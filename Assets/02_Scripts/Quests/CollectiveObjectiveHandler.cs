public class CollectObjectiveHandler : QuestObjectiveHandler
{
    public CollectObjectiveHandler(Quest quest, int index) : base(quest, index) { }

    public override void EvaluateProgress()
    {
        int count = 0;
        if (GameEvents.RequestPlayerItemCount != null)
        {
            count = GameEvents.RequestPlayerItemCount.Invoke(objectiveData.targetID);
        }

        quest.currentAmounts[objectiveIndex] = count;
    }

    //public override string GetProgressText()
    //{
    //    return $"{objectiveData.targetID} ¡¶√‚: {quest.currentAmounts[objectiveIndex]} / {objectiveData.requiredAmount}";
    //}
}