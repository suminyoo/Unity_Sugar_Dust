using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class CounterUI : MonoBehaviour
{
    private Action<bool> onTransactionCompleted;
    private Action onExitPressed;

    private CustomerBrain currentCustomer;

    // 생성된 돈 오브젝트를 관리할 리스트
    private List<GameObject> spawnedMoneyObjects = new List<GameObject>();

    [Header("UI Components")]
    public GameObject counterRootPanel;
    public Image itemIconImage;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemAmountText;
    public TextMeshProUGUI itempriceText;
    public TextMeshProUGUI totalPriceText;

    [Header("Buttons")]
    public Button confirmButton;
    public Button refuseButton;
    public Button exitButton;

    [Header("Money")]
    public Transform customerMoneyBoard; // Grid Layout Group이 있는 부모
    public GameObject moneyPrefab;       // 돈 프리팹
    public MoneyDatabase moneyDatabase;

    private List<DraggableMoney> customerMonies = new List<DraggableMoney>();
    private List<DraggableMoney> playerMonies = new List<DraggableMoney>();

    [Header("Zones")]
    public RectTransform customerMoneyZone;
    public RectTransform playerMoneyZone;
    public Transform dragLayer;   // 드래그 시 돈이 가려지지 않게 할 최상위 부모
    
    [Header("Stacking Settings")]
    public Vector2 stackStartPos = new Vector2(-150, 100); // 각 구역의 왼쪽 위 시작점
    public Vector2 stackOffset = new Vector2(10, -10);     // 쌓이는 간격

    private void Awake()
    {
        confirmButton.onClick.AddListener(OnClickConfirm);
        refuseButton.onClick.AddListener(OnClickRefuse);
        exitButton.onClick.AddListener(OnClickExit);

        counterRootPanel.SetActive(false);
        //counterTransactionPanel.SetActive(false);
    }

    public void ShowCounterUI(CustomerBrain customer, Action<bool> onComplete, Action onExit)
    {
        Cursor.visible = true;
        currentCustomer = customer;

        this.onTransactionCompleted = onComplete;
        this.onExitPressed = onExit;

        counterRootPanel.SetActive(true);
        //counterTransactionPanel.SetActive(true);
        confirmButton.interactable = true;
        refuseButton.interactable = true;

        // 데이터 바인딩
        if (customer.ItemToBuy != null)
        {
            itemIconImage.sprite = customer.ItemToBuy.icon;
            itemIconImage.gameObject.SetActive(true);
            itemNameText.text = customer.ItemToBuy.itemName;
            itempriceText.text = $"등록한 가격: {customer.ItemToBuyPrice:N0}G";
            itemAmountText.text = $"가져온 개수: {customer.ItemToBuyAmount}개";
            totalPriceText.text = $"받은 금액: {customer.ItemToBuyPrice * customer.ItemToBuyAmount:N0}G";
        }

        // 돈 표시 함수 호출
        DisplayCustomerMoney(customer.OfferedMoneyList, customer.MyType);
    }


    public void DisplayCustomerMoney(List<int> moneyList, CustomerType type)
    {
        ClearAllMoney(); // 청소

        bool useScatterStyle = UnityEngine.Random.value > 0.5f;

        // 사기 확률
        float currentFakeProbability = 0f;

        if (type == CustomerType.Scammer)
        {
            // 사기꾼: 20% ~ 80% 확률로 가짜돈
            currentFakeProbability = UnityEngine.Random.Range(0.2f, 0.8f);
            Debug.Log($"[사기꾼 등장] 가짜 돈 비율: {currentFakeProbability * 100:F0}%");
        }

        int index = 0;
        foreach (int amount in moneyList)
        {
            SpawnMoneyInternal(amount, customerMoneyZone, customerMonies, currentFakeProbability, false, useScatterStyle, index);
            index++;
        }
    }

    public void OnClickGiveChange(int amount)
    {
        int index = playerMonies.Count;
        SpawnMoneyInternal(amount, playerMoneyZone, playerMonies, 0f, true, false, index);
    }

    //돈 생성 공통 로직
    private void SpawnMoneyInternal(int amount, RectTransform parentZone, List<DraggableMoney> targetList, float fakeProb, bool isMine, bool isScattered, int index)
    {
        GameObject moneyObj = Instantiate(moneyPrefab, parentZone);
        DraggableMoney script = moneyObj.GetComponent<DraggableMoney>();
        if (script == null) script = moneyObj.AddComponent<DraggableMoney>();

        targetList.Add(script);

        bool isFake = false;

        if (!isMine && UnityEngine.Random.value <= fakeProb)
        {
            isFake = true;
        }

        var data = moneyDatabase.GetData(amount);
        Sprite spriteToUse;
        if (isMine)
        {
            // 내 돈이면 깨끗한 돈
            spriteToUse = moneyDatabase.GetFirstRealSprite(amount);
        }
        else
        {
            // 손님 돈이면 랜덤 (가짜 포함)
            spriteToUse = moneyDatabase.GetRandomSprite(amount, isFake);
        }
        // 초기화
        script.Initialize(amount, spriteToUse, data.size, dragLayer, isMine, parentZone, (deletedMoney) => {
            if (targetList.Contains(deletedMoney)) targetList.Remove(deletedMoney);
        });

        // 위치 잡기
        if (isScattered)
        {
            PlaceMoneyScattered(moneyObj.GetComponent<RectTransform>(), parentZone, data.size);
        }
        else
        {
            PlaceMoneyStacked(moneyObj.GetComponent<RectTransform>(), index);
        }
    }

    // 스태킹 위치 계산
    private void PlaceMoneyStacked(RectTransform itemRect, int index)
    {
        int loopCount = index % 10; // 10개씩 끊어서
        int colCount = index / 10;

        float x = stackStartPos.x + (loopCount * stackOffset.x) + (colCount * 50f);
        float y = stackStartPos.y + (loopCount * stackOffset.y);

        itemRect.localPosition = new Vector3(x, y, 0);
        itemRect.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-5f, 5f));
        itemRect.SetAsLastSibling();
    }
    private void PlaceMoneyScattered(RectTransform itemRect, RectTransform zoneRect, Vector2 itemSize)
    {
        // Zone의 크기
        float zoneW = zoneRect.rect.width;
        float zoneH = zoneRect.rect.height;

        // 아이템 크기
        float itemW = (itemSize.x > 0) ? itemSize.x : itemRect.rect.width;
        float itemH = (itemSize.y > 0) ? itemSize.y : itemRect.rect.height;

        // 경계선 안쪽 안전 구역 계산 (패딩 20 정도)
        float padding = 20f;
        float maxX = (zoneW / 2) - (itemW / 2) - padding;
        float maxY = (zoneH / 2) - (itemH / 2) - padding;

        if (maxX < 0) maxX = 0;
        if (maxY < 0) maxY = 0;

        // 랜덤 위치
        float x = UnityEngine.Random.Range(-maxX, maxX);
        float y = UnityEngine.Random.Range(-maxY, maxY);

        itemRect.localPosition = new Vector3(x, y, 0);

        // 랜덤 회전: -45도 ~ 45도
        itemRect.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-45f, 45f));

        itemRect.SetAsLastSibling();
    }

    // 수락 버튼
    private void OnClickConfirm()
    {
        if (currentCustomer == null) return;

        // A. 받아야 할 돈 (물건값)
        int itemPrice = currentCustomer.ItemToBuyPrice * currentCustomer.ItemToBuyAmount;

        // B. 손님이 낸 돈 총액 (CustomerZone에 있는 돈 합계)
        int customerPayTotal = CalculateTotal(customerMonies);

        // C. 내가 준 거스름돈 총액 (PlayerZone에 있는 돈 합계)
        int playerChangeTotal = CalculateTotal(playerMonies);

        // D. 검증: (낸 돈 - 물건값) == 거스름돈
        int requiredChange = customerPayTotal - itemPrice;

        if (currentCustomer.MyType == CustomerType.Tipper && playerChangeTotal <= requiredChange)
        {
            requiredChange = playerChangeTotal;
        }

        // 장부에 기록 
        if (SalesManager.Instance != null)
        {
            SalesManager.Instance.RecordTransaction(itemPrice, requiredChange, playerChangeTotal);
        }

        onTransactionCompleted?.Invoke(true);

        ClearAllMoney();
    }

    // 리스트 합계 계산 헬퍼
    private int CalculateTotal(List<DraggableMoney> list)
    {
        int sum = 0;
        foreach (var m in list) sum += m.amount;
        return sum;
    }

    public void ShowWaitingUI(Action onExit)
    {
        Cursor.visible = true;
        currentCustomer = null;
        this.onExitPressed = onExit;

        counterRootPanel.SetActive(false);
        //counterTransactionPanel.SetActive(false); // 대기 중엔 거래창 끄기

        confirmButton.interactable = false;
        refuseButton.interactable = false;

    }
    // 구역 청소
    private void ClearAllMoney()
    {
        foreach (var money in customerMonies) 
            if (money != null) Destroy(money.gameObject);
        customerMonies.Clear();

        foreach (var money in playerMonies)
            if (money != null) Destroy(money.gameObject);
        playerMonies.Clear();
    }

    public void CloseCounterUI()
    {
        counterRootPanel.SetActive(false);
        //counterTransactionPanel.SetActive(false);

        ClearAllMoney();

        currentCustomer = null;
        onTransactionCompleted = null;
        onExitPressed = null;

        // 돈 오브젝트 정리
        foreach (var obj in spawnedMoneyObjects) Destroy(obj);
        spawnedMoneyObjects.Clear();
    }

    private void OnClickRefuse()
    {
        if (currentCustomer == null) return;
        onTransactionCompleted?.Invoke(false);
    }

    private void OnClickExit()
    {
        onExitPressed?.Invoke();
    }
}