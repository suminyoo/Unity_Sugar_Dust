using UnityEngine;
using System.Collections.Generic;

public enum EnemyID //순서나 이름 수정 불가
{
    None,
    Enemy1_BlueSlime,
    Enemy2_BlueMonster,
    Enemy3_GreenSlime,
    Enemy4_StarMonster,
    Enemy5_RedSlime,
    Enemy6_PinkMonster
}

public class EnemyDataManager : MonoBehaviour
{
    public static EnemyDataManager Instance;

    private Dictionary<EnemyID, EnemyData> enemyDatabase = new Dictionary<EnemyID, EnemyData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadAllEnemies();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAllEnemies()
    {
        EnemyData[] enemies = Resources.LoadAll<EnemyData>("Enemy");
        foreach (var enemy in enemies)
        {
            // enum 기본값 체크
            if (enemy.enemyID == EnemyID.None) continue;

            if (!enemyDatabase.ContainsKey(enemy.enemyID))
            {
                enemyDatabase.Add(enemy.enemyID, enemy);
            }
        }


    }
    public EnemyData GetEnemyByID(EnemyID id)
    {
        if (id == EnemyID.None) return null;
        return enemyDatabase.TryGetValue(id, out EnemyData data) ? data : null;
    }

}