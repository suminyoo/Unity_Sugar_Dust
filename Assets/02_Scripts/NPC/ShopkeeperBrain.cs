using System.Collections;
using UnityEngine;

// [핵심] NPCBrain을 상속받습니다!
public class ShopkeeperBrain : NPCBrain
{
    [Header("Shop Settings")]
    public ShopData shopData; // 이 상점이 팔 물건들 (Inspector에서 할당)

    protected void Start()
    {
        base.Start();

        // 기본 패트롤 없음

        // TODO: 상점 주인 대기 모션 (근데 지금 없는)
        // controller.Animation.PlayIdle(ShopIdle); 
    }

    public override void HandleInteraction()
    {
        // 중복 실행 방지
        if (isInteracting) return;

        // 코루틴 시작
        StartCoroutine(ShopProcess());
    }

    // 상호작용
    private IEnumerator ShopProcess()
    {
        PrepareInteraction();
        yield return StartCoroutine(DialogueProcess());

        //상점 로직 
        Debug.Log($"[{shopData.shopName}] 상점 창 오픈!");

        // 실제로는 이런 식으로 호출하게 됩니다:
        // ShopUIManager.Instance.Open(shopData, () => isShopClosed = true);

        // [임시 시뮬레이션] 상점 UI가 열려있는 척 대기
        bool isShopClosed = false;

        // 테스트용: 2초 뒤에 상점을 닫는다고 가정
        // 나중에는 실제 UI가 닫힐 때 콜백을 받으면 됩니다.
        controller.Bubble.ShowBubble("천천히 구경해도 돼.", 2f);
        yield return new WaitForSeconds(2.0f); // 상점 구경 중...

        // 상점이 닫힐 때까지 대기 (나중에 주석 풀고 사용)
        // yield return new WaitUntil(() => isShopClosed);

        FinishInteraction();
    }
}