using System;

// 퀘스트용 static 클래스
public static class GameEvents
{

    public static Action<QuestID> OnQuestCompleted;

    public static Action OnQuestProgressUpdated;

    public static Func<ItemID, int> RequestPlayerItemCount;

    public static Action<EnemyID> OnEnemyKilled;

    public static Action<NPCID> OnNPCTalked;

    public static Action<PointID> OnPointArrived;

    public static Action<int> OnRevenueEarned;

    public static Func<int> RequestPlayerMoney;
}