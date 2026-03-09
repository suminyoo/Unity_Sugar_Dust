public class HuntObjectiveHandler : QuestObjectiveHandler
{
    public HuntObjectiveHandler(Quest quest, int index) : base(quest, index) { }

    public override void OnStart()
    {
        GameEvents.OnMonsterKilled += HandleMonsterKilled;
    }

    public override void OnStop()
    {
        GameEvents.OnMonsterKilled -= HandleMonsterKilled;
    }

    private void HandleMonsterKilled(string enemyID)
    {
        if (enemyID == objectiveData.targetID.ToString())
        {
            quest.currentAmounts[objectiveIndex]++;

            GameEvents.OnQuestProgressUpdated?.Invoke();
        }
    }

    public override void EvaluateProgress()
    {
    }

    //public override string GetProgressText()
    //{
    //    return $"{objectiveData.targetID} óġ: {quest.currentAmounts[objectiveIndex]} / {objectiveData.requiredAmount}";
    //}
}