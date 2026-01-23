using System.Collections;
using UnityEngine;

public class ShopkeeperBrain : NPCBrain
{
    private NPCShop myShop;


    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();

        myShop = GetComponent<NPCShop>();

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

        bool isShopping = true;

        //상점 로직 
        Debug.Log($" 상점 창 오픈");

        // 스토리지 매니저 호출
        StorageUIManager.Instance.OpenStorage(myShop, myShop.GetShopType(),
            () => { isShopping = false; } // 콜백(Action)
        );
        yield return new WaitForSeconds(0.5f);

        yield return new WaitWhile(() => isShopping);

        ShowGoodbyeMessage();
        FinishInteraction();
    }
}