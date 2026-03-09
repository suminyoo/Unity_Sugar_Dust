using System.Collections.Generic;
using UnityEngine;

public enum NPCID //순서나 이름 수정 불가
{
    None = 0,
    Shopkeeper_Weapon1 = 10,

    Customer1 = 50,

    Default_NPC1 = 100,
    Default_NPC2 = 101,
}


public class NPCDataManager : MonoBehaviour
{
    public static NPCDataManager Instance;

    // itemID: Key, ItemData: Value
    private Dictionary<NPCID, NPCData> npcDatabase = new Dictionary<NPCID, NPCData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadAllNPCs();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAllNPCs()
    {
        NPCData[] npcs = Resources.LoadAll<NPCData>("NPC");
        foreach (var npc in npcs)
        {
            if (npc.npcID == NPCID.None) continue;

            if (!npcDatabase.ContainsKey(npc.npcID))
            {
                npcDatabase.Add(npc.npcID, npc);
            }
        }
    }

    public NPCData GetNPCByID(NPCID id)
    {
        if (id == NPCID.None) return null;
        return npcDatabase.TryGetValue(id, out NPCData data) ? data : null;
    }
}