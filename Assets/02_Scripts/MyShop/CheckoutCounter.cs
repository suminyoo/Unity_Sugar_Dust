using UnityEngine;
using System.Collections.Generic;

public class CheckoutCounter : MonoBehaviour
{
    [Header("Settings")]
    // [변경] 직접 배치할 줄 서는 포인트들 (0번이 계산대 바로 앞)
    public List<Transform> queuePoints;

    // 현재 줄 서 있는 손님 리스트
    private List<CustomerBrain> waitingQueue = new List<CustomerBrain>();

    // 1. 손님이 줄에 들어올 때
    // [변경] 리턴 타입을 Vector3? 로 변경하여 자리가 없으면 null을 반환
    public Vector3? JoinQueue(CustomerBrain customer)
    {
        // 이미 줄을 서 있는 경우 (위치만 반환)
        if (waitingQueue.Contains(customer))
        {
            return queuePoints[waitingQueue.IndexOf(customer)].position;
        }

        // [신규] 자리가 꽉 찼는지 확인
        if (waitingQueue.Count >= queuePoints.Count)
        {
            // 자리 없음 -> 거절
            return null;
        }

        // 자리 있음 -> 줄에 추가
        waitingQueue.Add(customer);

        // 내 순번(Index)에 해당하는 포인트 위치 반환
        return queuePoints[waitingQueue.Count - 1].position;
    }

    // 2. 손님이 줄에서 나갈 때
    public void LeaveQueue(CustomerBrain customer)
    {
        if (waitingQueue.Contains(customer))
        {
            waitingQueue.Remove(customer);
            // 빠진 자리를 메우기 위해 뒤 사람들을 땡김
            UpdateQueuePositions();
        }
    }

    // 3. 줄 위치 재정렬
    private void UpdateQueuePositions()
    {
        for (int i = 0; i < waitingQueue.Count; i++)
        {
            // i번째 손님은 i번째 포인트로 이동
            Vector3 newPos = queuePoints[i].position;
            bool isFront = (i == 0);

            waitingQueue[i].UpdateQueueTarget(newPos, isFront);
        }
    }

    // 4. 플레이어 상호작용
    public void OnInteract()
    {
        if (waitingQueue.Count > 0)
        {
            CustomerBrain frontCustomer = waitingQueue[0];
            if (frontCustomer.IsReadyForTransaction)
            {
                frontCustomer.StartDialogueWithPlayer();
            }
        }
        else
        {
            Debug.Log("줄 선 손님이 없습니다.");
        }
    }
}