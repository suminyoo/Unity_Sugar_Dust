using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CounterUIManager : MonoBehaviour
{
    public static CounterUIManager Instance;
    private Action<bool> onTransactionCompleted;
    private Action onExitPressed;

    private CustomerBrain currentCustomer;

    [Header("UI Components")]
    public GameObject counterRootPanel;
    public GameObject counterTransactionPanel;
    public Image itemIconImage;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemAmountText;
    public TextMeshProUGUI itempriceText;
    public TextMeshProUGUI totalPriceText; 

    [Header("Buttons")]
    public Button confirmButton;
    public Button refuseButton;
    public Button exitButton;


    private void Awake()
    {
        if (Instance == null) Instance = this;

        confirmButton.onClick.AddListener(OnClickConfirm);
        refuseButton.onClick.AddListener(OnClickRefuse);
        exitButton.onClick.AddListener(OnClickExit);

        counterRootPanel.SetActive(false);
        counterTransactionPanel.SetActive(false);
    }

    public void ShowCounterUI(CustomerBrain customer, Action<bool> onComplete, Action onExit)
    {
        currentCustomer = customer;

        this.onTransactionCompleted = onComplete;
        this.onExitPressed = onExit;

        counterRootPanel.SetActive(true);
        counterTransactionPanel.SetActive(true);
        confirmButton.interactable = true;
        refuseButton.interactable = true;

        // 데이터 바인딩
        if (customer.ItemToBuy != null)
        {
            itemIconImage.sprite = customer.ItemToBuy.icon;
            itemIconImage.gameObject.SetActive(true);
            itemNameText.text = customer.ItemToBuy.itemName;
            itempriceText.text = $"등록한 가격: {customer.ItemToBuyPrice:N0}G";
            itemAmountText.text = $"가져온 개수: {customer.ItemToBuyAmount.ToString()}개";
            totalPriceText.text = $"받은 금액: {customer.ItemToBuyPrice * customer.ItemToBuyAmount}G";

        }
    }

    public void ShowWaitingUI(Action onExit)
    {
        currentCustomer = null;
        this.onExitPressed = onExit;

        counterRootPanel.SetActive(true);
        counterTransactionPanel.SetActive(false);

        // 거래 버튼 잠금
        confirmButton.interactable = false;
        refuseButton.interactable = false;
    }

    public void CloseCounterUI()
    {
        counterRootPanel.SetActive(false);
        counterTransactionPanel.SetActive(false);

        currentCustomer = null;
        onTransactionCompleted = null; // 연결 해제
        onExitPressed = null;
    }

    private void OnClickConfirm()
    {
        if (currentCustomer == null) return;
        onTransactionCompleted?.Invoke(true);
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