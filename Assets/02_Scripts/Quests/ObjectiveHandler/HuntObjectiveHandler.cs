public class HuntObjectiveHandler : QuestObjectiveHandler
{
    public HuntObjectiveHandler(Quest quest, int index) : base(quest, index) { }
    public override void OnStart() => GameEvents.OnEnemyKilled += HandleMonsterKilled;
    public override void OnStop() => GameEvents.OnEnemyKilled -= HandleMonsterKilled;

    private void HandleMonsterKilled(EnemyID enemyID)
    {
        if (enemyID == objectiveData.enemyID)
        {
            quest.currentAmounts[objectiveIndex]++;
            GameEvents.OnQuestProgressUpdated?.Invoke();
        }
    }

    public override void EvaluateProgress() { }
}