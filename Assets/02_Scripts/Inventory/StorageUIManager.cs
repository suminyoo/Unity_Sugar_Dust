using System;
using UnityEngine;
using static Unity.VisualScripting.Member;


// 두 인벤토리 간의 아이템 이동 및 UI 관리를 담당하는 매니저 클래스   
public class StorageUIManager : MonoBehaviour
{
    public static StorageUIManager Instance;

    private Action onCloseCallback;

    [Header("Player")]
    public InventoryUI playerInventoryUI; // 플레이어 인벤토리
    public GameObject playerBagPanel;     // 플레이어 인벤토리 전체를 감싸는 부모 패널 (토글 권한 위해)

    [Header("Target UI Panels")]
    public GameObject rootCanvas;

    public GameObject myShopPanel;
    public GameObject npcShopPanel;
    public GameObject commonStoragePanel;

    [Header("Target UIs ")] //상대스토리지가 사용할 ui들
    public InventoryUI myShopUI;
    public InventoryUI npcShopUI;
    public InventoryUI commonStorageUI; // 일반 상자용 UI

    // UI 전체를 감싸는 부모 오브젝트 (배경 등)

    // 현재 활성화된 데이터와 UI 변수
    private InventoryHolder _playerHolder;
    private InventoryHolder _currentOtherHolder; // 현재 거래 중인 Storage
    private InventoryUI _currentOtherUI;         // 현재 켜져 있는 Storage UI
    private GameObject _currentOtherPanel;       // 현재 켜져 있는 Storage UI를 자식으로 두는 패널

    [Header("Others")]
    public float closeDistance = 3.0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        rootCanvas.SetActive(false);

        _playerHolder = GameObject.FindGameObjectWithTag("Player").GetComponent<InventoryHolder>();

        // 시작할 때 모든 상대방 UI 비활성화
        if (myShopUI) myShopUI.gameObject.SetActive(false);
        if (npcShopUI) npcShopUI.gameObject.SetActive(false);
        if (commonStorageUI) commonStorageUI.gameObject.SetActive(false);

    }

    private void Update()
    {
        if (rootCanvas.activeSelf)
        {
            // ESC 키로 닫기
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseStorage();
                return;
            }

            // 거리 체크 (멀어지면 닫기)
            if (_currentOtherHolder != null && _playerHolder != null)
            {
                float dist = Vector3.Distance(_playerHolder.transform.position, _currentOtherHolder.transform.position);
                if (dist > closeDistance)
                {
                    CloseStorage();
                }
            }
        }
    }

    // 외부(Storage)에서 호출해서 UI 열기
    // TODO: uiType -  Enum을 써서 상자별 다른 ui를 열도록 확장 가능
    public void OpenStorage(InventoryHolder other,  string shopType = "Common", Action onClosed = null)
    {
        _currentOtherHolder = other;

        // 닫힐 때 실행할 행동
        this.onCloseCallback = onClosed;

        rootCanvas.SetActive(true);

        //플레이어 가방 강제로열기 
        playerBagPanel.SetActive(true);

        // 플레이어 UI 연결 및 갱신
        playerInventoryUI.connectedInventory = _playerHolder;
        playerInventoryUI.SetInventorySystem(_playerHolder.InventorySystem);

        CloseOtherStoragePanels();

        _currentOtherUI = null;
        _currentOtherPanel = null;

        // 타입에 따라 UI 패널 선택
        switch (shopType)
        {
            case "MyShop":
                _currentOtherUI = myShopUI;
                _currentOtherPanel = myShopPanel;
                if (other is IShopSource source)
                    _currentOtherUI.InitShopMode(source, InventoryContext.MyShop);
                break;
                
            case "Weapon":
                _currentOtherUI = npcShopUI;
                _currentOtherPanel = npcShopPanel;
                if (other is IShopSource npcSource)
                    _currentOtherUI.InitShopMode(npcSource, InventoryContext.NPCShop);
                break;

            case "Common":
                _currentOtherUI = commonStorageUI;
                _currentOtherPanel = commonStoragePanel;
                if (_currentOtherUI != null)
                    _currentOtherUI.contextType = InventoryContext.Chest;
                break;

        }

        // 선택된 상대방 UI 활성화 및 데이터 연결
        if (_currentOtherUI != null && _currentOtherPanel != null)
        {
            _currentOtherPanel.SetActive(true);
            _currentOtherUI.gameObject.SetActive(true);

            _currentOtherUI.connectedInventory = _currentOtherHolder;
            _currentOtherUI.SetInventorySystem(_currentOtherHolder.InventorySystem);

            _currentOtherUI.RefreshUI();
        }
    }
    public void CloseOtherStoragePanels()
    {
        if (myShopPanel) myShopPanel.SetActive(false);
        if (npcShopPanel) npcShopPanel.SetActive(false);
        if (commonStoragePanel) commonStoragePanel.SetActive(false);
    }

    public void CloseStorage()
    {
        // 루트배경
        rootCanvas.SetActive(false);

        // 상대ui 끄기
        // TODO: 상점 배경?과 상대ui 수정 고려
        if (_currentOtherUI != null) _currentOtherUI.gameObject.SetActive(false);
        if (_currentOtherPanel != null) _currentOtherPanel.SetActive(false);

        // 플레이어 가방 끄기
        playerBagPanel.SetActive(false);

        _currentOtherHolder = null;
        _currentOtherUI = null;
        _currentOtherPanel = null;

        if (onCloseCallback != null)
        {
            onCloseCallback.Invoke(); // NPC에게 신호 보냄
            onCloseCallback = null;   // 한 번 썼으니 비움
        }
    }

    // 아이템 이동 요청 처리 (슬롯에서 호출)
    public void HandleItemTransfer(int slotIndex, InventoryUI clickedUI)
    {
        // 스토리지 안 열려있으면 무시
        if (!rootCanvas.activeSelf) return;

        // 상점에서는 아이템 이동 불가
        if (clickedUI.contextType == InventoryContext.NPCShop) return;

        InventoryHolder fromHolder = null;
        InventoryHolder toHolder = null;

        // 플레이어 UI를 클릭했는지
        if (clickedUI == playerInventoryUI)
        {
            fromHolder = _playerHolder;       // 플레이어
            toHolder = _currentOtherHolder;   // 현재 열린 스토리지


            // 플레이어 인벤토리에서 상점으로 아이템 이동 금지 (기부행동 레전드)
            if (_currentOtherUI != null && _currentOtherUI.contextType == InventoryContext.NPCShop) return;
        }
        // 현재열린상대방 UI를 클릭했느,ㄴ지
        else if (clickedUI == _currentOtherUI)
        {
            fromHolder = _currentOtherHolder; // 현재 열린 스토리지
            toHolder = _playerHolder;         // 플레이어
        }

        // 전송 실행
        if (fromHolder != null && toHolder != null)
        { 
            fromHolder.TransferTo(slotIndex, toHolder);
        }
    }
}