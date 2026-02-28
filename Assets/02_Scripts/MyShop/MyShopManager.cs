using UnityEngine;
using TMPro;

public class MyShopManager : MonoBehaviour
{
    public static MyShopManager Instance;

    [Header("Settings")]
    public float businessDuration = 120f;
    private float currentTime;
    public bool IsShopOpen { get; private set; } = false;

    [Header("References")]
    public CustomerSpawner spawner;
    public CheckoutCounter counter;
    public OpenCloseMyShop openCloseInteraction;
    public ClosingReceiptUI closingReceiptUI;

    public static bool IsShopMode = false; // 씬 체인지로 영업모드로 바뀔때

    public float GetCurrentTime() => currentTime;
    public float GetTimeLimit() => businessDuration;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        SalesManager.Instance.StartNewShop();

        if (IsShopMode)
        {
            RealShopMode();  //상점 영업 모드
            IsShopMode = false;
        }
        else
        {
            TempShopMode();  // 단순 내 상점 방문 (Additive)
        }
    }

    private void Update()
    {
        if (IsShopOpen)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                EndShopMode();
            }
        }
    }

    private void TempShopMode()
    {
        IsShopOpen = false;
    }

    public void RealShopMode()
    {
        IsShopOpen = true;

        NotificationUIManager.Instance.ShowNotification("영업이 시작되었습니다");

        currentTime = businessDuration;

        spawner.StartSpawning();

        openCloseInteraction.SetState(OpenCloseMyShop.MyShopState.SHOP_OPEN);
    }

    // 영업 시간 종료
    private void EndShopMode()
    {
        if (!IsShopOpen) return;
        IsShopOpen = false;

        NotificationUIManager.Instance.ShowNotification("영업이 종료되었습니다");

        spawner.StopSpawning();

        // 손님 내보내기
        counter.ClearQueueOnClose();
        CustomerBrain[] allCustomers = FindObjectsOfType<CustomerBrain>();
        foreach (var customer in allCustomers)
        {
            customer.ForceLeave();
        }
        openCloseInteraction.SetState(OpenCloseMyShop.MyShopState.SHOP_CLOSED);
    }

    public void OpenSettlementUI()
    {
        
        closingReceiptUI.ShowReceipt();
        
    }
    public void ForceEarlyClose()
    {
        if (!IsShopOpen) return;

        EndShopMode();

        NotificationUIManager.Instance.ShowNotification("영업을 조기 마감합니다.");
    }
}