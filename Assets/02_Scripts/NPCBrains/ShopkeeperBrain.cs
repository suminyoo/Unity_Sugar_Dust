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
    }


    protected override IEnumerator DefaultInteractionProcess()
    {
        PrepareInteraction();

        yield return StartCoroutine(DialogueProcess(showAutoGoodbye: false));

        GameEvents.OnNPCTalkedFinished?.Invoke(controller.npcData.npcID);

        bool isShopping = true;

        StorageUIManager.Instance.OpenStorage(myShop, myShop.GetShopType(),
            () => { isShopping = false; }
        );
        yield return new WaitForSeconds(0.5f);

        yield return new WaitWhile(() => isShopping);

        ShowGoodbyeMessage();
        FinishInteraction();
    }
}