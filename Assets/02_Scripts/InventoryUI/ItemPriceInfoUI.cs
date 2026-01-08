using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ItemPriceInfoUI : MonoBehaviour
{
    #region Variables & References

    [Header("Panels")]
    public GameObject itemInfoPanel;
    public GameObject defaultPanel;

    [Header("Item Info")]
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI sellingPriceText;

    [Header("Activation")]
    public Button activateButton;       // 활성화/비활성화 토글 버튼
    public TextMeshProUGUI buttonText;  // 버튼 글씨 ("판매 시작" / "판매 중지")
    public Image buttonImage;           // 버튼 색상 변경용

    public Color activePriceColor = new Color(0, 0.6f, 0, 0.8f); // 판매 중 색
    public Color inactivePriceColor = new Color(0, 0, 0, 0.5f);  // 판매 중지 색

    // 내부 상태 변수
    private int currentSellingPrice;
    private bool isCurrentActive;

    // 콜백 
    private Action<int> onPriceChanged;
    private Action<bool> onActiveChanged; 

    #endregion


    private void Start()
    {
        itemInfoPanel.SetActive(false);
        defaultPanel.SetActive(true);

        if (activateButton != null)
        {
            // 혹시라도 중복 연결되는 걸 막기 위해 기존 연결 삭제
            activateButton.onClick.RemoveAllListeners();

            // "클릭하면 OnActivateButtonClicked 함수를 실행해라"
            activateButton.onClick.AddListener(OnActivateButtonClicked);
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
        nameText.text = data.itemName;

        currentSellingPrice = currentPrice;
        isCurrentActive = isActive;

        // UI 갱신
        UpdateUI();
    }

    public void Close()
    {
        itemInfoPanel.SetActive(false);
        defaultPanel.SetActive(true);
    }


    // 판매 가격 변경 (+10, -10)
    public void ChangeSellingPrice(int amount)
    {
        currentSellingPrice += amount;
        if (currentSellingPrice < 0) currentSellingPrice = 0;

        UpdateUI();

        // 실시간 가격 반영
        onPriceChanged?.Invoke(currentSellingPrice);
    }

    // 판매 시작/중지 버튼 클릭
    private void OnActivateButtonClicked()
    {
        // 상태 뒤집기 (Toggle)
        isCurrentActive = !isCurrentActive;

        UpdateUI();

        // 실시간 상태 반영
        onActiveChanged?.Invoke(isCurrentActive);
    }

    private void UpdateUI()
    {
        // 가격 표시
        sellingPriceText.text = $"{currentSellingPrice:N0} G";

        // 활성화 버튼 상태 표시
        if (isCurrentActive)
        {
            buttonText.text = "판매 중지";
            buttonImage.color = activePriceColor; // 활성화됨 (초록색 등)
        }
        else
        {
            buttonText.text = "판매 시작";
            buttonImage.color = inactivePriceColor;  // 비활성화 (회색)
        }
    }


}