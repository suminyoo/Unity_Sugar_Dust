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
    public float dayTime = 120f; // 하루 제한 시간 (미정

    void Awake()
    {
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

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}