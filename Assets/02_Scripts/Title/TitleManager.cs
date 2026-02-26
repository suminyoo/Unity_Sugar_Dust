using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public SaveSlotUI[] saveSlots;

    public GameObject subSavePanel;
    public Transform subSaveContainer;
    public GameObject saveSlotPrefab;
    public GameObject emptyNoticeText;
    private int currentOpenListSlot = -1;

    private void Start()
    {
        RefreshSlots();
        subSavePanel.SetActive(false);
        emptyNoticeText.SetActive(false);
    }
    public void RefreshSlots()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            int slotNum = saveSlots[i].slotNumber;
            SaveMetadata latestData = GetLatestMetadata(slotNum);
            saveSlots[i].UpdateSlotUI(latestData, "", this, false);
        }
    }

    // 특정 슬롯 번호의 가장 최신 세이브 정보
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

    public void OnSlotClick(int slotNum)
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, $"Saves/Slot{slotNum}");

        if (Directory.Exists(directoryPath))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
            FileInfo[] files = dirInfo.GetFiles("*.json");

            if (files.Length > 0)
            {
                // 가장 최신 파일 경로를 찾아서 로드 함수에 전달
                string latestPath = files.OrderByDescending(f => f.LastWriteTime).First().FullName;
                LoadSaveFile(latestPath, slotNum);
                return;
            }
        }

        // 데이터가 없으면 새 게임
        StartNewGame(slotNum);
    }

    public void OnSaveListButtonClick(int arrayIndex)
    {
        int slotNum = saveSlots[arrayIndex].slotNumber;
        string directoryPath = Path.Combine(Application.persistentDataPath, $"Saves/Slot{slotNum}");

        if (subSavePanel.activeSelf && currentOpenListSlot == slotNum)
        {
            subSavePanel.SetActive(false);
            currentOpenListSlot = -1;
            return;
        }

        // 다른 슬롯을 눌렀거나 새로 서브리스트 여는 경우
        currentOpenListSlot = slotNum;
        subSavePanel.SetActive(true);

        // 리스트 새로고침
        foreach (Transform child in subSaveContainer)
        {
            Destroy(child.gameObject);
        }
        if (!Directory.Exists(directoryPath))
        {
            emptyNoticeText.SetActive(true);
            return;
        }
        FileInfo[] files = new DirectoryInfo(directoryPath).GetFiles("*.json");

        if (files.Length == 0)
        {
            emptyNoticeText.SetActive(true);
            return;
        }
        emptyNoticeText.SetActive(false);

        // 최신순으로 5개 생성
        var sortedFiles = files.OrderByDescending(f => f.LastWriteTime).Take(5);

        foreach (var file in sortedFiles)
        {
            GameObject item = Instantiate(saveSlotPrefab, subSaveContainer);
            SaveSlotUI ui = item.GetComponent<SaveSlotUI>();

            string json = File.ReadAllText(file.FullName);
            GameData data = JsonUtility.FromJson<GameData>(json);

            ui.UpdateSlotUI(data.metadata, file.FullName, this, true);
            ui.slotNumber = slotNum;
        }
    }

    private void StartNewGame(int slotNum)
    {
        GameSaveManager.Instance.InitData();
        GameSaveManager.Instance.currentSaveSlot = slotNum;
        GameSaveManager.Instance.SetTimerActive(true);
        SceneController.Instance.ChangeSceneAndAddScene(SCENE_NAME.TOWN, SCENE_NAME.PLAYER_HOME, SPAWN_ID.PLAYERHOME_BED, true);
    }

    public void LoadSaveFile(string fullPath, int slotNum)
    {
        string json = File.ReadAllText(fullPath);

        JsonUtility.FromJsonOverwrite(json, GameSaveManager.Instance.savedData);

        GameSaveManager.Instance.currentSaveSlot = slotNum;
        GameSaveManager.Instance.SetTimerActive(true);

        Debug.Log($"파일 로드 완료: {fullPath}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyLoadedData();
        }

        SceneController.Instance.ChangeSceneAndAddScene(SCENE_NAME.TOWN, SCENE_NAME.PLAYER_HOME, SPAWN_ID.PLAYERHOME_BED, true);

    }

    public void DeleteSlot(int arrayIndex)
    {
        CommonConfirmPopup.Instance.OpenPopup(
            "정말로 이 세이브를 삭제하시겠습니까?",
            () => {
                int slotNum = saveSlots[arrayIndex].slotNumber;
                string directoryPath = Path.Combine(Application.persistentDataPath, $"Saves/Slot{slotNum}");

                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                    RefreshSlots();
                }

                subSavePanel.SetActive(false);
                currentOpenListSlot = -1;
                RefreshSlots();
            }
        );

        
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }


}