using UnityEngine;

public class UpgradeTester : MonoBehaviour, IInteractable
{
    public void OnInteract()
    {
        if (!PlayerAssetsManager.Instance.CheckMoney(100)) return;

        CommonConfirmPopup.Instance.OpenPopup(
            "모든 능력치와 가방/진열대를 1단계 업그레이드 하시겠습니까? (비용: 100G)",
            () => {
                PerformUpgrade();
            }
        );
    }

    private void PerformUpgrade()
    {
        // 돈 차감
        PlayerAssetsManager.Instance.TrySpendMoney(100);

        // 체력 & 스테미나 레벨업
        var player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCondition>();

        // 현재 레벨을 가져와서 ++
        // 실제 구조에서는 player.UpgradeHp() 같은 함수로 사용

        // --- 테스트를 위한 강제 데이터 조작 ---
        var data = GameSaveManager.Instance.savedData;
        data.hpLevel++;
        data.staminaLevel++;
        data.inventorySizeLevel++;
        data.displayStandSizeLevel++;

        // 바뀐 레벨에 맞춰 최대 수치들 재계산
        player.LoadStatusFromManager();

        // 인벤토리/진열대 사이즈 실제 변경 적용 ui 업데이트

        // 데이터 보관
        player.SaveData();
        PlayerAssetsManager.Instance.SaveData();

        Debug.Log($"[UpgradeTest] 업그레이드 완료! HP레벨: {data.hpLevel}, 인벤레벨: {data.inventorySizeLevel}");
        NotificationUIManager.Instance.ShowNotification("모든 능력치가 강화되었습니다!");
    }

    public string GetInteractPrompt() => "테스트: 전체 업그레이드 (100G)";
}