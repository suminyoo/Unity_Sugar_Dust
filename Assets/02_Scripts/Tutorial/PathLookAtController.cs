using System;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine; // 시네마신 패키지 사용을 위해 필수입니다.

// 인스펙터에서 거리와 타겟을 한 쌍으로 묶어서 보기 위한 구조체
[Serializable]
public struct PathTarget
{
    [Tooltip("패스에서 이 거리 값을 넘어가면 타겟이 변경됩니다.")]
    public float distanceThreshold;

    [Tooltip("해당 거리에서 바라볼 대상 (병원, 무기상점 등)")]
    public Transform lookAtTarget;
}

public class PathLookAtController : MonoBehaviour
{
    [Header("시네마신 컴포넌트 연결")]
    [Tooltip("타겟을 변경할 시네마신 가상 카메라")]
    public CinemachineVirtualCamera vcam;

    [Tooltip("패스를 따라 이동하는 돌리 카트 (현재 거리를 추적하기 위함)")]
    public CinemachineDollyCart dollyCart;

    [Header("거리별 타겟 설정")]
    [Tooltip("주의: distanceThreshold 값이 '작은 순서에서 큰 순서(오름차순)'로 리스트를 작성해주세요.")]
    public List<PathTarget> targetList = new List<PathTarget>();

    void Update()
    {
        // 필수 컴포넌트나 타겟 리스트가 비어있으면 실행하지 않음
        if (vcam == null || dollyCart == null || targetList.Count == 0) return;

        // 돌리 카트의 현재 이동 거리를 가져옴
        float currentDistance = dollyCart.m_Position;

        // 기본 타겟을 리스트의 첫 번째 물체로 설정
        Transform newLookAtTarget = targetList[0].lookAtTarget;

        // 현재 거리를 설정된 값들과 비교하여 알맞은 타겟 탐색
        for (int i = 0; i < targetList.Count; i++)
        {
            if (currentDistance >= targetList[i].distanceThreshold)
            {
                newLookAtTarget = targetList[i].lookAtTarget;
            }
        }

        // 가상 카메라가 바라보고 있는 타겟이 방금 찾은 타겟과 다르다면 교체
        if (vcam.LookAt != newLookAtTarget)
        {
            vcam.LookAt = newLookAtTarget;
        }
    }
}