using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public SaveSlotUI[] saveSlots;

    private void Start()
    {
        RefreshSlots();
    }

    public void RefreshSlots()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            int slotNum = saveSlots[i].slotNumber;
            SaveMetadata latestData = GetLatestMetadata(slotNum);
            saveSlots[i].UpdateSlotUI(latestData);
        }
    }

    // 특정 슬롯 번호의 가장 최신 세이브 정보를 가져오기
    private SaveMetadata GetLatestMetadata(int slotNumber)
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, $"Saves/Slot{slotNumber}");

        // 폴더가 아예 없으면 세이브가 없는 것
        if (!Directory.Exists(directoryPath)) return null;

        DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
        FileInfo[] files = dirInfo.GetFiles("*.json");

        // 폴더는 있는데 파일이 없어도 세이브가 없는 것
        if (files.Length == 0) return null;

        // 가장 최근 파일 1개만 찾
        FileInfo latestFile = files.OrderByDescending(f => f.LastWriteTime).First();

        // 파일 읽어서 GameData로 변환한 뒤 metadata가져오기
        string json = File.ReadAllText(latestFile.FullName);
        GameData data = JsonUtility.FromJson<GameData>(json);

        if (data != null && data.metadata != null)
        {
            return data.metadata;
        }

        return null;
    }

    public void OnSlotClick(int arrayIndex)
    {
        int slotNum = saveSlots[arrayIndex].slotNumber;
        string directoryPath = Path.Combine(Application.persistentDataPath, $"Saves/Slot{slotNum}");

        // 세이브 폴더가 있고 파일이 있는지 확인 (이어하기)
        if (Directory.Exists(directoryPath))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
            FileInfo[] files = dirInfo.GetFiles("*.json");

            if (files.Length > 0)
            {
                FileInfo latestFile = files.OrderByDescending(f => f.LastWriteTime).First();
                string json = File.ReadAllText(latestFile.FullName);

                // GameSaveManager의 savedData에 덮어씌우기
                JsonUtility.FromJsonOverwrite(json, GameSaveManager.Instance.savedData);
                Debug.Log($"{slotNum}번 세이브 로드 완료.,게임 시작");

                GameSaveManager.Instance.currentSaveSlot = slotNum;
                SceneManager.LoadScene("TOWN");
                return;
            }
        }

        // 빈 슬롯(새 게임
        Debug.Log($"{slotNum}번 슬롯 빈 슬롯. 새 게임 시작");

        // 데이터 초기화
        GameSaveManager.Instance.InitData();
        GameSaveManager.Instance.currentSaveSlot = slotNum;
        SceneManager.LoadScene("TOWN");
    }
}