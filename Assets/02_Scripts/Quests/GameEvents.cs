using System;

// 퀘스트용 static 클래스
public static class GameEvents
{
    // 몬스터처치 이벤트 (몬스터 ID)
    public static Action<string> OnMonsterKilled;

    // 아이템획득 이벤트 (아이템 ID, 획득량)
    public static Action<string, int> OnItemCollected;

    // 장사수익 이벤트
    public static Action<int> OnRevenueEarned;

    // 탐사 레벨 도달 이벤트
    public static Action<int> OnLevelReached;
}