using System;

// 퀘스트용 static 클래스
public static class GameEvents
{
    public static Action OnQuestProgressUpdated;

    // 아이템 제출 퀘스트 이벤트
    public static Func<string, int> RequestPlayerItemCount;

    // 몬스터처치 이벤트 (몬스터 ID)
    public static Action<string> OnMonsterKilled;

    // 장사수익 이벤트
    public static Action<int> OnRevenueEarned;

    // 탐사 레벨 도달 이벤트
    public static Action<int> OnLevelReached;


}