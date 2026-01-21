using UnityEngine;
using System;
using System.Collections.Generic;

// ¿µ±¸ ÀÚ»ê(µ·, Æ¯¼ö ´É·Â, ÇØ±İ ¿ä¼Ò)À» °ü¸®ÇÏ´Â ¸Å´ÏÀú
public class PlayerAssetsManager : MonoBehaviour, ISaveable
{
    public static PlayerAssetsManager Instance { get; private set; }

    [SerializeField] private int currentMoney = 0;

    // Æ¯¼ö Àåºñ/´É·Â (Key Items) °ü¸® (Áßº¹ ¹æÁö¸¦ À§ÇØ HashSet »ç¿ë)
    // ¿¹: "Translator", "RunningShoes" µîÀÇ ¹®ÀÚ¿­ ID·Î °ü¸®
    //°¡±¸ Áõ¼­? µîµî
    private HashSet<string> ownedKeyItems = new HashSet<string>();

    // UI ¾÷µ¥ÀÌÆ®¸¦ À§ÇÑ ÀÌº¥Æ®
    // TODO: »óÁ¡ UI°¡ ¿Ï¼ºµÇ¸é ÀÌº¥Æ®¸¦ ±¸µ¶ÇÏ¿© µ· °ªÀÌ º¯ÇÒ ¶§¸¶´Ù È­¸éÀÇ ÅØ½ºÆ®°¡ °»½ÅµÇµµ·Ï

    public event Action<int> OnMoneyChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // µ¥ÀÌÅÍ ·Îµå
        if (GameSaveManager.Instance != null)
        {
            var data = GameSaveManager.Instance.LoadPlayerAssets();
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
        Debug.Log($"[Money] {amount}¿ø È¹µæ (ÇöÀç: {currentMoney})");
    }

    public bool TrySpendMoney(int amount)
    {
        if (CheckMoney(amount))
        {
            currentMoney -= amount;
            OnMoneyChanged?.Invoke(currentMoney);
            return true;
        }
        return false;
    }

    public bool CheckMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            return true;
        }
        else
        {
            Debug.Log("µ·ÀÌ ºÎÁ·ÇÕ´Ï´Ù");
            //TODO: µ· ºÎÁ·ÇÏ´Ù´Â ¾Ë¸²Ã¢ ¸Ş¼¼Áö º¸³»¼­ ¶ç¿ì´Â Å¬·¡½º ±¸Çö
            return false;

        }
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

    public void SaveData()
    {
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SavePlayerAssets(currentMoney, ownedKeyItems);
        }
    }
}



