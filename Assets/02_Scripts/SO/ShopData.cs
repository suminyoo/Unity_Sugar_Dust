using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Shop Data")]
public class ShopData : ScriptableObject
{
    public string shopName;
    public List<ShopItemInfo> itemsForSale; // 판매 아이템 목록
}

// 상점 진열 아이템의 상세 정보를 정의
[System.Serializable]
public class ShopItemInfo
{
    public ItemData itemData;

    [Header("Settings")]
    public int priceOverride = -1; // -1이면 아이템 기본 가격 사용
    // 바가지나 할인에 사용할것. 여기에 적힌 가격으로 적용됨

    public int stockCount = 1; //판매할 수량 (초기설정)

    public bool isInfinite = false; //재고 무한
}