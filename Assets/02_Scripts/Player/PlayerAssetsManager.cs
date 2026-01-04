using UnityEngine;
using System;
using System.Collections.Generic;

// ¿µ±¸ ÀÚ»ê(µ·, Æ¯¼ö ´É·Â, ÇØ±İ ¿ä¼Ò)À» °ü¸®ÇÏ´Â ¸Å´ÏÀú
public class PlayerAssetsManager : MonoBehaviour
{
    public static PlayerAssetsManager Instance { get; private set; }

    [SerializeField] private int currentMoney = 0;


    // Æ¯¼ö Àåºñ/´É·Â (Key Items) °ü¸® (Áßº¹ ¹æÁö¸¦ À§ÇØ HashSet »ç¿ë)
    // ¿¹: "Translator", "RunningShoes" µîÀÇ ¹®ÀÚ¿­ ID·Î °ü¸®
    //°¡±¸ Áõ¼­? µîµî
    private HashSet<string> ownedKeyItems = new HashSet<string>();



    // UI ¾÷µ¥ÀÌÆ®¸¦ À§ÇÑ ÀÌº¥Æ®
    public event Action<int> OnMoneyChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 1. °ÔÀÓ ½ÃÀÛ ½Ã µ¥ÀÌÅÍ ·Îµå
        if (GameManager.Instance != null)
        {
            var data = GameManager.Instance.LoadAssets();
            currentMoney = data.money;

            // ÀúÀåµÈ Å° ¾ÆÀÌÅÛ º¹±¸
            foreach (var item in data.keyItems)
            {
                ownedKeyItems.Add(item);
            }

            // UI ÃÊ±âÈ­ ÀÌº¥Æ® È£Ãâ
            OnMoneyChanged?.Invoke(currentMoney);
        }
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);
        Debug.Log($"[Money] {amount}¿ø È¹µæ! (ÇöÀç: {currentMoney})");
    }

    // ³ªÁß¿¡ ÇÃ·¹ÀÌ¾î°¡ »óÁ¡À» È®ÀåÇÏ°Å³ª °¡±¸¸¦ »ì ¶§ »ç¿ë
    public bool TrySpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            OnMoneyChanged?.Invoke(currentMoney);
            return true;
        }
        return false;
    }


    // Æ¯¼ö ¾ÆÀÌÅÛ(Key Items)=========================
    // ¿µ±¸ ¾ÆÀÌÅÛ È¹µæ (¿¹: Åë¿ª±â ±¸¸Å)
    public void UnlockKeyItem(string itemID)
    {
        if (!ownedKeyItems.Contains(itemID))
        {
            ownedKeyItems.Add(itemID);
            Debug.Log($"[Assets] ¿µ±¸ ¾ÆÀÌÅÛ È¹µæ: {itemID}");
            // ÇÊ¿äÇÏ´Ù¸é ¿©±â¼­ ÀúÀå ½Ã½ºÅÛ È£Ãâ
        }
    }

    // ¾ÆÀÌÅÛ º¸À¯ ¿©ºÎ È®ÀÎ (¿¹: ¿Ü°è¾î ÇØ¼® °¡´É ¿©ºÎ ®G)
    public bool HasKeyItem(string itemID)
    {
        return ownedKeyItems.Contains(itemID);
    }
}



