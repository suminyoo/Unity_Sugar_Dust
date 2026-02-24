using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    public TextMeshProUGUI saveTimeText;
    public TextMeshProUGUI inGameDayText;
    public TextMeshProUGUI inGameTimeText;
    public TextMeshProUGUI playTimeText;
    public GameObject emptySlotText;
    public int slotNumber;

    public void UpdateSlotUI(SaveMetadata data)
    {
        if (data == null)
        {
            // 데이터가 없으면 빈 슬롯 처리
            emptySlotText.SetActive(true);
            saveTimeText.gameObject.SetActive(false);
            inGameDayText.gameObject.SetActive(false);
            inGameTimeText.gameObject.SetActive(false);
            playTimeText.gameObject.SetActive(false);
        }
        else
        {
            inGameDayText.text = $"{data.inGameDay}일차 - ";

            string timeText = "";
            switch (data.inGameTime)
            {
                case GAME_TIME.Morning: timeText = "아침"; break;
                case GAME_TIME.Day: timeText = "낮"; break;
                case GAME_TIME.Evening: timeText = "저녁"; break;
                case GAME_TIME.Night: timeText = "밤"; break;
            }

            // 데이터가 있으면 텍스트 업데이트
            emptySlotText.SetActive(false);
            saveTimeText.gameObject.SetActive(true);
            inGameDayText.gameObject.SetActive(true);
            inGameTimeText.gameObject.SetActive(true);
            playTimeText.gameObject.SetActive(true);

            saveTimeText.text = data.saveTime;
            inGameTimeText.text = timeText;

            int hours = Mathf.FloorToInt(data.playTime / 3600);
            int minutes = Mathf.FloorToInt((data.playTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(data.playTime % 60);
            playTimeText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
    }
}