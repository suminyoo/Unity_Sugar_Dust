using UnityEngine;
using System.Collections.Generic;

public class CheckoutCounter : MonoBehaviour, IInteractable
{
    public List<Transform> queuePoints; // 계산 줄 위치들

    // 계산줄 리스트
    private List<CustomerBrain> waitingQueue = new List<CustomerBrain>();
    public string GetInteractPrompt() => "[E] 물건 결제해주기";

    // 상호작용
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
            Debug.Log("줄 선 손님이 없습니다");
        }
    }

    // 줄에 들어옴
    // 자리가 없으면 null
    public Vector2? JoinQueue(CustomerBrain customer)
    { 
        // 자리 꽉 찼는지 확인
        if (waitingQueue.Count >= queuePoints.Count)
        {
            return null;
        }

        waitingQueue.Add(customer);

        // Index에 해당하는 위치 반환
        return queuePoints[waitingQueue.Count - 1].position;
    }

    // 줄에서 나감
    public void LeaveQueue(CustomerBrain customer)
    {
        if (waitingQueue.Contains(customer))
        {
            waitingQueue.Remove(customer);
            UpdateQueuePositions();
        }
    }

    // 줄 위치 재정렬
    private void UpdateQueuePositions()
    {
        for (int i = 0; i < waitingQueue.Count; i++)
        {
            // 이동
            Vector3 newPos = queuePoints[i].position;
            bool isFront = (i == 0);

            waitingQueue[i].UpdateQueueTarget(newPos, isFront);
        }
    }

    // 영업 종료
    public void ClearQueueOnClose()
    {
        List<CustomerBrain> tmpQueue = new List<CustomerBrain>(waitingQueue);

        foreach (var customer in tmpQueue)
        {
            if (customer.IsInteracting)
            {
                continue;
            }

            LeaveQueue(customer);

            // 강제 퇴장
            customer.ForceLeave(dropItem: true);
        }
    }

}