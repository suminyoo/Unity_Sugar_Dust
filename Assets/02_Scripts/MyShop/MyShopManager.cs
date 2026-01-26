using UnityEngine;
using System.Collections;
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

    public TextMeshProUGUI timerText;

    public static bool IsShopMode = false; // 씬 체인지로 영업모드로 바뀔때

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
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
            UpdateTimeUI();

            if (currentTime <= 0)
            {
                EndShopMode();
            }
        }
    }
    public void UpdateTimeUI()
    {
        currentTime -= Time.deltaTime;

        if (timerText != null)
        {
            float displayTime = Mathf.Max(currentTime, 0);
            int minutes = Mathf.FloorToInt(displayTime / 60F);
            int seconds = Mathf.FloorToInt(displayTime % 60F);
            timerText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
        }
    }
    private void TempShopMode()
    {
        IsShopOpen = false;
        if (timerText != null) timerText.text = "";

    }

    public void RealShopMode()
    {
        IsShopOpen = true;

        NotificationUIManager.Instance.ShowNotification("영업이 시작되었습니다");

        currentTime = businessDuration;

        // 스포너 가동
        spawner.StartSpawning();

        openCloseInteraction.SetState(OpenCloseMyShop.MyShopState.SHOP_OPEN);
    }

    // 영업 시간 종료
    private void EndShopMode()
    {
        IsShopOpen = false;

        NotificationUIManager.Instance.ShowNotification("영업이 종료되었습니다");

        if (timerText != null) timerText.text = "CLOSED";

        spawner.StopSpawning();

        StartCoroutine(CloseShopProcess());
    }

    // 마감 프로세스
    private IEnumerator CloseShopProcess()
    {
        // 줄 클리어
        counter.ClearQueueOnClose();

        // 손님 내보내기
        CustomerBrain[] allCustomers = FindObjectsOfType<CustomerBrain>();
        foreach (var customer in allCustomers)
        {
            if (!customer.IsInteracting && !customer.IsInQueue)
            {
                customer.ForceLeave();
            }
        }

        openCloseInteraction.SetState(OpenCloseMyShop.MyShopState.SHOP_CLOSED);

        yield return null;
    }

}