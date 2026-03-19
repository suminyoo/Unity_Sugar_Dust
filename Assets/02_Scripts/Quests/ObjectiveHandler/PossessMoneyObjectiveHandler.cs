
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

