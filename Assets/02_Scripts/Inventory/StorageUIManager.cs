using UnityEngine;

// 두 인벤토리 간의 아이템 이동 및 UI 관리를 담당하는 매니저 클래스   
public class StorageUIManager : MonoBehaviour
{
    public static StorageUIManager Instance;

    [Header("Player")]
    public InventoryUI playerInventoryUI; // 플레이어 인벤토리
    public GameObject playerBagPanel;     // 플레이어 인벤토리 전체를 감싸는 부모 패널 (토글 권한 위해)

    [Header("Target UIs ")] //상대스토리지가 사용할 ui들
    public InventoryUI myShopUI;
    public InventoryUI commonStorageUI; // 일반 상자용 UI
    public InventoryUI weaponShopUI; 

    // UI 전체를 감싸는 부모 오브젝트 (배경 등)
    public GameObject rootCanvas;

    // 현재 활성화된 데이터와 UI 변수
    private InventoryHolder _playerHolder;
    private InventoryHolder _currentOtherHolder; // 현재 거래 중인 Storage
    private InventoryUI _currentOtherUI;         // 현재 켜져 있는 Storage UI

    public float closeDistance = 3.0f;

    private void Awake()
    {
        Instance = this;
        rootCanvas.SetActive(false);

        // 시작할 때 모든 상대방 UI 비활성화
        if (commonStorageUI) commonStorageUI.gameObject.SetActive(false);
        if (myShopUI) myShopUI.gameObject.SetActive(false);
        if (weaponShopUI) weaponShopUI.gameObject.SetActive(false);
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
    public void OpenStorage(InventoryHolder player, InventoryHolder other, string shopType = "Common")
    {
        _playerHolder = player;
        _currentOtherHolder = other;

        rootCanvas.SetActive(true);

        //플레이어 가방 강제로열기 
        playerBagPanel.SetActive(true);

        // 플레이어 UI 연결 및 갱신
        playerInventoryUI.connectedInventory = _playerHolder;
        playerInventoryUI.SetInventorySystem(_playerHolder.InventorySystem);

        // 상대방 UI 선택 로직 (확장 포인트)
        // 기존 열려있던 UI 끄기
        if (_currentOtherUI != null) _currentOtherUI.gameObject.SetActive(false);

        // 타입에 따라 UI 패널 선택
        switch (shopType)
        {
            case "MyShop":
                _currentOtherUI = myShopUI;
                if (other is IShopSource source)
                {
                    _currentOtherUI.InitShopMode(source, InventoryContext.MyShop);
                }
                break;

                // 나중에 NPC 상점 추가할 때 예시
                /*
                case "Weapon":
                    _currentOtherUI = weaponShopUI;
                    if (other is IShopSource npcSource)
                        _currentOtherUI.InitShopMode(npcSource, InventoryContext.NPCShop);
                    break;
                */
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
        // 루트배경
        rootCanvas.SetActive(false);

        // 상대ui 끄기
        // TODO: 상점 배경?과 상대ui 수정 고려
        _currentOtherUI.gameObject.SetActive(false);

        // 플레이어 가방 끄기
        playerBagPanel.SetActive(false);

        //_playerHolder = null;
        _currentOtherHolder = null;
        _currentOtherUI = null;
    }

    // 아이템 이동 요청 처리 (슬롯에서 호출)
    public void HandleItemTransfer(int slotIndex, InventoryUI clickedUI)
    {
        // 스토리지?> 샵이 안 열려있으면 무시
        if (!rootCanvas.activeSelf) return;

        InventoryHolder fromHolder = null;
        InventoryHolder toHolder = null;

        // 플레이어 UI를 클릭했는지
        if (clickedUI == playerInventoryUI)
        {
            fromHolder = _playerHolder;       // 플레이어
            toHolder = _currentOtherHolder;   // 현재 열린 스토리지
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
            // 여기서 NPC 상점이면 이동 막기 가능
            // 그러면 클릭 구매만 되는것임 (구현 고려)
            // 예: if (clickedUI.contextType == InventoryContext.NPCShop) return;

            fromHolder.TransferTo(slotIndex, toHolder);
        }
    }
}