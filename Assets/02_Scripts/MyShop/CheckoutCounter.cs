using UnityEngine;
using System.Collections.Generic;

public class CheckoutCounter : MonoBehaviour, IInteractable
{
    public TransactionUI transactionUI;
    private CameraFollow mainCamera;
    public Transform counterViewPoint; //카운터뷰

    public List<Transform> queuePoints; // 계산 줄 위치들
    public List<CustomerBrain> waitingQueue = new List<CustomerBrain>(); // 계산줄 리스트

    public float transitionSpeed = 5.0f;

    private bool isCounterMode = false; 
    private bool isTransactionActive = false; // 지금 계산 중인지

    public string GetInteractPrompt() => LocalizationHelper.Main("PROMPT_CHECKOUT");

    // 상호작용
    public void OnInteract()
    {
        if (isCounterMode) return;

        if (waitingQueue.Count == 0)
        {
            NotificationUIManager.Instance.ShowNotification(LocalizationHelper.Main("NOTI_NO_WAITING_CUSTOMER"));
            return;
        }

        StartCounterMode();
    }

    #region Unity Lifecycle

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
    private void OnDisable()
    {
        if (isCounterMode)
        {
            if (PauseManager.openPopupCount > 0)
                PauseManager.openPopupCount--;

            isCounterMode = false;
        }
    }

    #endregion

    #region Counter Mode Management

    private void StartCounterMode()
    {
        isCounterMode = true;
        isTransactionActive = false;

        PauseManager.openPopupCount++;

        InputControlManager.Instance.LockInput();
        mainCamera.StartOverrideView(counterViewPoint, transitionSpeed);
        transactionUI.ShowWaitingUI(() => StopCounterMode());

        if (waitingQueue.Count > 0)
        {
            CustomerBrain frontCustomer = waitingQueue[0];
            if (frontCustomer.IsReadyForTransaction)
            {
                TryStartTransaction(frontCustomer);
            }
        }
    }

    // 나가기 버튼
    public void StopCounterMode()
    {
        if (!isCounterMode) return;

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.EndDialogue();

        isCounterMode = false;
        isTransactionActive = false;

        // UI 끄기
        transactionUI.CloseCounterUI();

        // 카메라 복귀
        mainCamera.ExitOverrideView();

        InputControlManager.Instance.UnlockInput();

        StartCoroutine(DecreasePopupCountDelayed());
    }

    private System.Collections.IEnumerator DecreasePopupCountDelayed()
    {
        yield return new WaitForEndOfFrame();

        if (PauseManager.openPopupCount > 0)
        {
            PauseManager.openPopupCount--;
        }
    }

    #endregion

    #region Transaction Management

    private void TryStartTransaction(CustomerBrain customer)
    {
        if (!isCounterMode || isTransactionActive) return;
        if (waitingQueue.Count > 0 && waitingQueue[0] == customer)
        {
            StartTransaction(customer);
        }
    }

    // 거래 시작
    private void StartTransaction(CustomerBrain customer)
    {
        isTransactionActive = true;

        customer.StartTransactionDialogue();

        // UI에 손님 정보 표시
        transactionUI.ShowCounterUI(
            customer,
            (isSuccess) => HandleTransactionResult(customer, isSuccess), // 거래 완료 시
            () => StopCounterMode() // 나가기 시
        );
    }

    // 거래 결과 처리
    private void HandleTransactionResult(CustomerBrain customer, bool isSuccess)
    {
        customer.OnTransactionDialogueFinished(isSuccess);

        // 카운터 상태 초기화
        isTransactionActive = false;
        transactionUI.ShowWaitingUI(() => StopCounterMode());
    }

    #endregion

    #region Queue Management

    // 줄에 들어옴
    // 자리가 없으면 null
    public (Vector3 position, Quaternion rotation)? JoinQueue(CustomerBrain customer)
    { 
        // 자리 꽉 찼는지
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
            customer.ForceLeave();
        }
    }
    #endregion

}