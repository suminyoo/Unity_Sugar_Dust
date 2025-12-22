using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Economy")]
    public int money = 100;
    public int totalOreCount = 0;
    public float currentWeight = 0f;

    [Header("Life Cycle")]
    public int dayCount = 1;
    public float dayTime = 120f; // 하루 제한 시간 (초)

    void Awake()
    {
        // 씬이 바뀌어도 파괴되지 않는 싱글톤 구조
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddMoney(int amount) { money += amount; }
    public void SpendMoney(int amount) { money -= amount; }

    // 씬 전환 시 호출할 함수
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}