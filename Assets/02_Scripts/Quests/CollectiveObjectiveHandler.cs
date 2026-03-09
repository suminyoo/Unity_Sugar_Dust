using static UnityEditor.Progress;

public class CollectObjectiveHandler : QuestObjectiveHandler
{
    // 부모 클래스 초기화를 위해 base 생성자 호출. 함수 : base 문법
    public CollectObjectiveHandler(Quest quest, int index) : base(quest, index) { }

    public override void EvaluateProgress()
    {
        int count = 0;
        if (GameEvents.RequestPlayerItemCount != null)
        {
            count = GameEvents.RequestPlayerItemCount.Invoke(objectiveData.targetID.itemID.ToString());
        }

        quest.currentAmounts[objectiveIndex] = count;
    }

    //public override string GetProgressText()
    //{
    //    return $"{objectiveData.targetID} 제출: {quest.currentAmounts[objectiveIndex]} / {objectiveData.requiredAmount}";
    //}
}