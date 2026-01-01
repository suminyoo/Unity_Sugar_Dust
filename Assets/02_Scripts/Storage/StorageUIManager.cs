using UnityEngine;

public class StorageUIManager : MonoBehaviour
{
    public static StorageUIManager Instance;

    [Header("Player UI")]
    public InventoryUI playerInventoryUI; // 플레이어 인벤토리

    [Header("Various Target UIs (상대방 UI들)")]
    public InventoryUI commonStorageUI; // 일반 상자/진열대용 기본 UI
    public InventoryUI myShopUI;
    public InventoryUI weaponShopUI; //이런식 추가

    // UI 전체를 감싸는 부모 오브젝트 (배경 등)
    public GameObject rootCanvas;

    // ★ 현재 활성화된 데이터와 UI를 기억하는 변수 ★
    private InventoryHolder _playerHolder;
    private InventoryHolder _currentOtherHolder; // 현재 거래 중인 상대방 (진열대, 상자 등)
    private InventoryUI _currentOtherUI;         // 현재 켜져 있는 상대방 UI

    private void Awake()
    {
        Instance = this;
        rootCanvas.SetActive(false);

        // 시작할 때 모든 상대방 UI를 꺼둡니다.
        if (commonStorageUI) commonStorageUI.gameObject.SetActive(false);
        if (myShopUI) myShopUI.gameObject.SetActive(false);
        if (weaponShopUI) weaponShopUI.gameObject.SetActive(false);
    }

    // 1. 외부(진열대, 상자)에서 호출하는 열기 함수
    // uiType 같은 Enum을 써서 어떤 모양의 UI를 열지 결정할 수도 있습니다.
    public void OpenStorage(InventoryHolder player, InventoryHolder other, string shopType = "Common")
    {
        _playerHolder = player;
        _currentOtherHolder = other;

        rootCanvas.SetActive(true);

        // 플레이어 UI 연결 및 갱신
        playerInventoryUI.connectedInventory = _playerHolder;
        playerInventoryUI.SetInventorySystem(_playerHolder.InventorySystem);

        // 상대방 UI 선택 로직 (확장 포인트)
        // 기존 열려있던 UI 끄기
        if (_currentOtherUI != null) _currentOtherUI.gameObject.SetActive(false);

        // 타입에 따라 적절한 UI 패널 선택
        switch (shopType)
        {
            case "MyShop":
                _currentOtherUI = myShopUI;
                break;
            case "Weapon":
                _currentOtherUI = weaponShopUI;
                break;
            case "Common":
                break;
                
            default:
                _currentOtherUI = commonStorageUI;
                break;
        }

        // 선택된 상대방 UI 활성화 및 데이터 연결
        if (_currentOtherUI != null)
        {
            _currentOtherUI.gameObject.SetActive(true);
            _currentOtherUI.connectedInventory = _currentOtherHolder;
            _currentOtherUI.SetInventorySystem(_currentOtherHolder.InventorySystem);
        }
    }

    public void CloseStorage()
    {
        rootCanvas.SetActive(false);
        if (_currentOtherUI != null) _currentOtherUI.gameObject.SetActive(false);

        _playerHolder = null;
        _currentOtherHolder = null;
        _currentOtherUI = null;
    }

    // 2. 아이템 이동 요청 처리 (슬롯에서 호출)
    public void HandleItemTransfer(int slotIndex, InventoryUI clickedUI)
    {
        // 샵이 안 열려있으면 무시
        if (!rootCanvas.activeSelf) return;

        InventoryHolder fromHolder = null;
        InventoryHolder toHolder = null;

        // A. 플레이어 UI를 클릭했나요?
        if (clickedUI == playerInventoryUI)
        {
            fromHolder = _playerHolder;       // 나: 플레이어
            toHolder = _currentOtherHolder;   // 너: 현재 열린 상대방 (진열대든, 무기상이든)
        }
        // B. 현재 열린 상대방 UI를 클릭했나요?
        else if (clickedUI == _currentOtherUI)
        {
            fromHolder = _currentOtherHolder; // 나: 상대방
            toHolder = _playerHolder;         // 너: 플레이어
        }

        // 전송 실행
        if (fromHolder != null && toHolder != null)
        {
            fromHolder.TransferTo(slotIndex, toHolder);
        }
    }
}