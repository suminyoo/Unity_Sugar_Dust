using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PathTarget
{
    public float distanceThreshold; 
    public Transform lookAtTarget;
}

public class PathLookAtController : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    public List<PathTarget> targetList = new List<PathTarget>();
    public float moveSpeed = 1f;

    private CinemachineTrackedDolly trackedDolly;
    private int currentTargetIndex = -1;

    void Awake()
    {
        if (vcam != null)
        {
            trackedDolly = vcam.GetCinemachineComponent<CinemachineTrackedDolly>();
        }
    }

    // 외부(TutorialManager)에서 호출할 코루틴
    public IEnumerator MoveToNextTarget()
    {
        currentTargetIndex++;
        if (currentTargetIndex >= targetList.Count) yield break;

        PathTarget target = targetList[currentTargetIndex];

        // 바라보는 대상 변경
        vcam.LookAt = target.lookAtTarget;

        // 지정된 distanceThreshold까지 Dolly 위치 이동
        while (trackedDolly.m_PathPosition < target.distanceThreshold)
        {
            trackedDolly.m_PathPosition += Time.deltaTime * moveSpeed;
            // 목표치를 살짝 넘어가면 고정
            if (trackedDolly.m_PathPosition > target.distanceThreshold)
            {
                trackedDolly.m_PathPosition = target.distanceThreshold;
            }
            yield return null;
        }
    }

    // 튜토리얼 리셋 시 사용
    public void ResetPath()
    {
        currentTargetIndex = -1;
        if (trackedDolly != null) trackedDolly.m_PathPosition = 0;
    }
}