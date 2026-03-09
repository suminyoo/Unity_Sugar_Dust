using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct DigitController
{
    public TextMeshProUGUI digitText; // 해당 자리의 숫자 텍스트
    public Button upButton;           // 위 버튼 (+1)
    public Button downButton;         // 아래 버튼 (-1)
}

public class ItemPriceInfoUI : MonoBehaviour
{
    #region Variables & References

    [Header("Panels")]
    public GameObject itemInfoPanel;
    public GameObject defaultPanel;

    [Header("Item Info")]
    public Image icon;
    public TextMeshProUGUI nameText;

    [Header("Activation")]
    public Button activateButton;       // 활성화 비활성화 토글 버튼
    public TextMeshProUGUI buttonText;  // 버튼 글씨 (판매 시작 / 판매 중지)
    public Image buttonImage;           // 버튼 색상용

    public Color activePriceColor = new Color(0, 0.6f, 0, 0.8f);
    public Color inactivePriceColor = new Color(0, 0, 0, 0.5f);

    // 콜백 
    private Action<int> onPriceChanged;
    private Action<bool> onActiveChanged;

    [Header("Price")]
    public DigitController[] digitControllers = new DigitController[3];
    private int currentSellingPrice;
    private int[] currentDigits = new int[3];
    private bool isCurrentActive;
    public TextMeshProUGUI currencyText;
    #endregion


    private void Start()
    {
        itemInfoPanel.SetActive(false);
        defaultPanel.SetActive(true);

        // 기존 연결 삭제
        activateButton.onClick.RemoveAllListeners();
        activateButton.onClick.AddListener(OnActivateButtonClicked);

        if (currencyText != null)
        {
            currencyText.text = CustomerPaymentSystem.CURRENCY_SYMBOL;
        }

        for (int i = 0; i < digitControllers.Length; i++)
        {
            int index = i;

            if (digitControllers[i].upButton != null)
                digitControllers[i].upButton.onClick.AddListener(() => ChangeDigit(index, 1));

            if (digitControllers[i].downButton != null)
                digitControllers[i].downButton.onClick.AddListener(() => ChangeDigit(index, -1));
        }
    }

    public void OpenPanel(ItemData data, int currentPrice, bool isActive,
                          Action<int> onPriceCallback, Action<bool> onActiveCallback)
    
    {
        onPriceChanged = onPriceCallback;
        onActiveChanged = onActiveCallback;

        defaultPanel.SetActive(false);
        itemInfoPanel.SetActive(true);

        // 기본 정보 표시
        icon.sprite = data.icon;
        nameText.text = data.GetItemName();

        currentSellingPrice = Mathf.Clamp(currentPrice, 0, 9999);
        currentSellingPrice = (currentSellingPrice / 10) * 10;

        isCurrentActive = isActive;
        ExtractDigits(currentSellingPrice);

        // UI 갱신
        UpdateUI();
    }

    public void Close()
    {
        itemInfoPanel.SetActive(false);
        defaultPanel.SetActive(true);
    }


    private void ChangeDigit(int index, int amount)
    {
        if (isCurrentActive) return;

        currentDigits[index] += amount;

        // 0~9 순환
        if (currentDigits[index] > 9) currentDigits[index] = 0;
        else if (currentDigits[index] < 0) currentDigits[index] = 9;

        // 최종 가격
        currentSellingPrice = (currentDigits[0] * 100 + currentDigits[1] * 10 + currentDigits[2]) * 10;

        UpdateUI();
    }

    private void OnActivateButtonClicked()
    {
        isCurrentActive = !isCurrentActive;

        UpdateUI();

        // 판매를 시작할 때 설정된 최종 금액
        if (isCurrentActive)
        {
            onPriceChanged?.Invoke(currentSellingPrice);
        }

        onActiveChanged?.Invoke(isCurrentActive);
    }

    private void ExtractDigits(int price)
    {
        currentDigits[0] = (price / 1000) % 10;
        currentDigits[1] = (price / 100) % 10;
        currentDigits[2] = (price / 10) % 10;
    }

    private void UpdateUI()
    {
        for (int i = 0; i < digitControllers.Length; i++)
        {
            digitControllers[i].digitText.text = currentDigits[i].ToString();

            digitControllers[i].upButton.interactable = !isCurrentActive;
            digitControllers[i].downButton.interactable = !isCurrentActive;
        }

        // 활성화 버튼
        if (isCurrentActive)
        {
            buttonText.text = LocalizationHelper.L("ITEM_INFO_STOP_SALE");
            buttonImage.color = activePriceColor;
        }
        else
        {
            buttonText.text = LocalizationHelper.L("ITEM_INFO_START_SALE");
            buttonImage.color = inactivePriceColor;
        }
    }
}
