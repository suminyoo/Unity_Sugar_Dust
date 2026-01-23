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
    public GameObject timesUpPanel;

    public static bool IsShopMode = false; // 씬 체인지로 영업모드로 바뀔때

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (IsShopMode)
        {
            //상점 영업 모드
            StartShop();
            IsShopMode = false;
        }
        else
        {
            // 단순 내 상점 방문 (Additive)
            InitialShopMode();
        }
    }

    private void Update()
    {
        if (IsShopOpen)
        {
            UpdateTimeUI();

            // 시간 종료 체크
            if (currentTime <= 0)
            {
                EndShop();
            }
        }
    }
    public void UpdateTimeUI()
    {
        //시간 감소
        currentTime -= Time.deltaTime;

        if (timerText != null)
        {
            float displayTime = Mathf.Max(currentTime, 0);
            int minutes = Mathf.FloorToInt(displayTime / 60F);
            int seconds = Mathf.FloorToInt(displayTime % 60F);
            timerText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
        }
    }
    private void InitialShopMode()
    {
        IsShopOpen = false;
        if (timerText != null) timerText.text = "";
        if (timesUpPanel != null) timesUpPanel.SetActive(false);

    }

    public void StartShop()
    {
        IsShopOpen = true;
        currentTime = businessDuration;

        // 스포너 가동
        spawner.StartSpawning();

        openCloseInteraction.SetState(OpenCloseMyShop.MyShopState.SHOP_OPEN);
    }

    // 영업 시간 종료
    private void EndShop()
    {
        IsShopOpen = false;
        if (timerText != null) timerText.text = "CLOSED";
        if (timesUpPanel != null) timesUpPanel.SetActive(true);

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
            // 아직 거래 중이지 않고, 줄도 안 섰다면 바로 퇴장
            if (!customer.IsInteracting && !customer.IsInQueue)
            {
                customer.ForceLeave(dropItem: true); // 물건 들고 있었으면 떨구고 감
            }
        }

        // 마지막 손님(계산 중인 사람)이 나갈 때까지 대기하거나,
        // 플레이어가 문을 클릭할 수 있게 상태 변경
        openCloseInteraction.SetState(OpenCloseMyShop.MyShopState.SHOP_CLOSED);

        yield return null;
    }

    // 4. 플레이어가 문을 눌러서 완전히 나가려고 할 때 체크
    public bool CanExitShop()
    {
        // 아직 손님이 남아있는지 확인
        CustomerBrain[] remainingCustomers = FindObjectsOfType<CustomerBrain>();

        if (remainingCustomers.Length > 0)
        {
            // (옵션) 강제로 다 삭제하거나, 기다리라는 메시지 띄우기
            Debug.Log("아직 손님이 남아있습니다. 다 나갈 때까지 기다리세요.");
            return false;
        }

        return true;
    }
}