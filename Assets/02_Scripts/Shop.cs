using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpgradeInventory()
    {
        // 현재 레벨 가져오기 (GameManager에서)
        int currentLv = GameManager.Instance.savedData.inventorySize;

        // 다음 레벨 사이즈 확인
        int nextLv = currentLv + 1;
        // 최대 레벨 체크 등 필요

        // GameManager 데이터 업데이트 (즉시 저장)
        GameManager.Instance.savedData.inventorySize = nextLv;

        // 현재 인벤토리 시스템 확장
        // (방법1 현재 아이템 임시 저장 -> 새 사이즈로 시스템 생성 -> 아이템 다시 넣기)
        // 혹은 InventorySystem 내부에 ExpandSlots() 같은 함수를 만들기?

        Debug.Log("가방 업그레이드 완료!");
    }
}
