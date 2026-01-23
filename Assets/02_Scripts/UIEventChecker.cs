using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIEventChecker : MonoBehaviour
{
    void Update()
    {
        // 마우스 왼쪽 버튼을 눌렀을 때 실행
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();

            // 현재 마우스 위치에서 모든 UI 레이캐스트 실행
            if (EventSystem.current != null)
            {
                EventSystem.current.RaycastAll(pointerData, results);

                if (results.Count > 0)
                {
                    Debug.Log("--- UI 클릭 감지 목록 ---");
                    foreach (var result in results)
                    {
                        // 클릭된 오브젝트의 이름과 레이어를 출력
                        Debug.Log($"[클릭됨]: {result.gameObject.name} | 레이어: {result.gameObject.layer}");
                    }
                }
                else
                {
                    Debug.Log("클릭된 UI가 없습니다.");
                }
            }
        }
    }
}