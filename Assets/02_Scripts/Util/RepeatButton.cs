using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Events;

// 버튼을 꾹 누르고 있으면 이벤트를 반복해서 실행해주는 유틸
public class RepeatButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Settings")]
    public float initialDelay = 0.5f; // 처음 눌렀을 때 대기
    public float repeatInterval = 0.1f; // 반복 간격

    public UnityEvent onPressed; // 인스펙터에서 연결할 함수

    private Coroutine repeatCoroutine;

    public void OnPointerDown(PointerEventData eventData)
    {
        // 좌클릭만 허용
        if (eventData.button != PointerEventData.InputButton.Left) return;

        onPressed?.Invoke();
        repeatCoroutine = StartCoroutine(RepeatRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 손을 떼면 코루틴 중지
        if (repeatCoroutine != null)
        {
            StopCoroutine(repeatCoroutine);
            repeatCoroutine = null;
        }
    }

    // 버튼 밖으로 나가면 중지
    public void OnPointerExit(PointerEventData eventData)
    {
        StopRepeat();
    }

    // 오브젝트 꺼지면 중지
    private void OnDisable()
    {
        StopRepeat();
    }
    private void StopRepeat()
    {
        if (repeatCoroutine != null)
        {
            StopCoroutine(repeatCoroutine);
            repeatCoroutine = null;
        }
    }

    private IEnumerator RepeatRoutine()
    {
        // 딜레이만큼 대기 후 실행
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            // 계속 누르고 있는지 검사
            if (!Input.GetMouseButton(0))
            {
                StopRepeat();
                yield break;
            }
            // 반복
            onPressed?.Invoke();
            yield return new WaitForSeconds(repeatInterval);
        }
    }
}