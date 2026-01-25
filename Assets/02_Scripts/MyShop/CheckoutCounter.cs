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

    // 상호작용
    public void OnInteract()
    {
        if (isCounterMode) return;

        StartCoroutine(StartCounterWorkRoutine());
    }

    private IEnumerator StartCounterWorkRoutine()
    {
        isCounterMode = true;
        isTransactionActive = false;

        // 입력 잠금 및 카메라 이동
        if (InputControlManager.Instance != null) InputControlManager.Instance.LockInput();
        if (mainCamera != null) mainCamera.StartOverrideView(counterViewPoint, transitionSpeed);

        // 3. UI를 '대기 상태'로 켬
        CounterUIManager.Instance.ShowWaitingUI();


        // 4. 영업 루프 시작 (나가기 버튼 누를 때까지 계속 돔)
        while (isCounterMode)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // 거래 중이면 강제 종료 처리 혹은 무시
                // 여기서는 깔끔하게 '나가기 버튼' 누른 것과 똑같이 처리합니다.
                StopWorking();
                yield break; // 코루틴 즉시 종료
            }

            // 계산 중이 아니고, 줄 선 손님이 있다면
            if (!isTransactionActive && waitingQueue.Count > 0)
            {
                CustomerBrain frontCustomer = waitingQueue[0];

                // 맨 앞 손님이 카운터 앞에 완전히 도착했는지 확인
                if (frontCustomer != null && frontCustomer.IsReadyForTransaction)
                {
                    BeginTransaction(frontCustomer);
                }
            }

            // 손님이 없거나 오고 있으면 대기
            yield return null;
        }
    }

    // 거래 시작 (손님 대면)
    private void BeginTransaction(CustomerBrain customer)
    {
        isTransactionActive = true; // 중복 실행 방지

        // 대화 시작
        customer.StartTransactionDialogue();

        // UI에 손님 정보 표시
        CounterUIManager.Instance.ShowCounterUI(
            customer,
            (isSuccess) => HandleTransactionResult(customer, isSuccess), // 거래 완료 시 실행할 행동
            () => StopWorking() // 나가기 버튼 누르면 실행할 행동
        );
    }
    // 거래 결과를 처리하는 함수 (UI 이벤트에 의해 호출됨)
    private void HandleTransactionResult(CustomerBrain customer, bool isSuccess)
    {
        // 1. 돈 처리 및 로그
        if (isSuccess)
        {
            int totalGain = customer.ItemToBuyPrice * customer.ItemToBuyAmount;
            Debug.Log($"[수익] {totalGain} G 벌었습니다!");
            // GameManager.Instance.AddMoney(totalGain);
        }
        else
        {
            Debug.Log("[거절] 거래를 거절했습니다.");
        }

        // 2. 손님 반응 (대화 종료 및 퇴장 준비)
        customer.OnTransactionDialogueFinished(isSuccess);

        // 3. 카운터 상태 초기화 (다음 손님 받기)
        CompleteCurrentTransaction();
    }

    // ★ UI에서 [판매/거절] 버튼을 눌렀을 때 호출됨
    public void CompleteCurrentTransaction()
    {
        // 거래 상태 해제 -> 루프(while)에서 다음 손님을 찾게 됨
        isTransactionActive = false;

        // UI를 다시 대기 모드로 변경
        CounterUIManager.Instance.ShowWaitingUI();
    }

    // ★ UI에서 [나가기] 버튼을 눌렀을 때 호출됨 (영업 종료)
    public void StopWorking()
    {
        isCounterMode = false; // while 루프 종료 조건

        StartCoroutine(ExitCounterRoutine());
    }

    private IEnumerator ExitCounterRoutine()
    {
        // UI 끄기
        CounterUIManager.Instance.CloseCounterUI();

        // 카메라 복귀
        if (mainCamera != null) mainCamera.ExitOverrideView();

        // 입력 잠금 해제
        if (InputControlManager.Instance != null) InputControlManager.Instance.UnlockInput();

        yield return null;
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

        Transform targetPoint = queuePoints[waitingQueue.Count - 1];

        // Index에 해당하는 위치 반환
        return (targetPoint.position, targetPoint.rotation);
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