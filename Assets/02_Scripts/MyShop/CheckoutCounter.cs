using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CheckoutCounter : MonoBehaviour, IInteractable
{
    // 카메라
    private CameraFollow mainCamera;

    public Transform counterViewPoint; //카운터뷰
    public float transitionSpeed = 5.0f;

    public List<Transform> queuePoints; // 계산 줄 위치들
    public List<CustomerBrain> waitingQueue = new List<CustomerBrain>(); // 계산줄 리스트

    private bool isCounterMode = false; 
    private bool isTransactionActive = false; // 지금 계산 중인가? (대화/UI 떠있는 상태)

    public string GetInteractPrompt() => "[E] 물건 결제해주기";

    private void Start()
    {
        mainCamera = Camera.main.GetComponent<CameraFollow>();
    }
    private void Update()
    {
        if (isCounterMode && Input.GetKeyDown(KeyCode.Escape))
        {
            StopCounterMode();
        }
    }

    // 상호작용
    public void OnInteract()
    {
        if (isCounterMode) return;
        StartCounterMode();
    }

    private void StartCounterMode()
    {
        isCounterMode = true;
        isTransactionActive = false;

        // 입력 잠금 및 카메라 이동
        InputControlManager.Instance.LockInput();
        mainCamera.StartOverrideView(counterViewPoint, transitionSpeed);

        CounterUIManager.Instance.ShowWaitingUI(() => StopCounterMode());

        if (waitingQueue.Count > 0)
        {
            CustomerBrain frontCustomer = waitingQueue[0];
            if (frontCustomer.IsReadyForTransaction)
            {
                TryStartTransaction(frontCustomer);
            }
        }
    }

    private void TryStartTransaction(CustomerBrain customer)
    {
        if (!isCounterMode) return;
        if (isTransactionActive) return;
        if (waitingQueue.Count > 0 && waitingQueue[0] == customer)
        {
            BeginTransaction(customer);
        }
    }


    // 거래 시작
    private void BeginTransaction(CustomerBrain customer)
    {
        isTransactionActive = true;

        customer.StartTransactionDialogue();

        // UI에 손님 정보 표시
        CounterUIManager.Instance.ShowCounterUI(
            customer,
            (isSuccess) => HandleTransactionResult(customer, isSuccess), // 거래 완료 시 실행할 행동
            () => StopCounterMode() // 나가기 버튼 누르면 실행할 행동
        );
    }

    // 거래 결과 처리
    private void HandleTransactionResult(CustomerBrain customer, bool isSuccess)
    {
        if (isSuccess)
        {
            int totalGain = customer.ItemToBuyPrice * customer.ItemToBuyAmount;
            PlayerAssetsManager.Instance.AddMoney(totalGain);
        }
        customer.OnTransactionDialogueFinished(isSuccess);

        // 카운터 상태 초기화
        isTransactionActive = false;
        CounterUIManager.Instance.ShowWaitingUI(() => StopCounterMode());
    }

    // 나가기 버튼
    public void StopCounterMode()
    {
        isCounterMode = false; // while 루프 종료 조건

        // UI 끄기
        CounterUIManager.Instance.CloseCounterUI();

        // 카메라 복귀
        if (mainCamera != null) mainCamera.ExitOverrideView();

        // 입력 잠금 해제
        InputControlManager.Instance.UnlockInput();
    }

    // 줄에 들어옴
    // 자리가 없으면 null
    public (Vector3 position, Quaternion rotation)? JoinQueue(CustomerBrain customer)
    { 
        // 자리 꽉 찼는지 확인
        if (waitingQueue.Count >= queuePoints.Count)
        {
            return null;
        }

        waitingQueue.Add(customer);
        customer.OnArrivedAtCounter += TryStartTransaction;

        Transform targetPoint = queuePoints[waitingQueue.Count - 1];

        // Index에 해당하는 위치 반환
        return (targetPoint.position, targetPoint.rotation);
    }

    // 줄에서 나감
    public void LeaveQueue(CustomerBrain customer)
    {
        if (waitingQueue.Contains(customer))
        {
            customer.OnArrivedAtCounter -= TryStartTransaction;

            waitingQueue.Remove(customer);
            UpdateQueuePositions();
        }
    }

    // 줄 위치 재정렬
    private void UpdateQueuePositions()
    {
        for (int i = 0; i < waitingQueue.Count; i++)
        {
            bool isFront = (i == 0);
            waitingQueue[i].UpdateQueueTarget(queuePoints[i].position, queuePoints[i].rotation, isFront);
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