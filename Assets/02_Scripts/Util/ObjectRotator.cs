using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    [Header("회전 방향")]
    public Vector3 rotationAxis = Vector3.up;

    [Tooltip("회전 속도")]
    public float rotationSpeed = 10f;

    void Update()
    {
        transform.Rotate(rotationAxis * (rotationSpeed * Time.deltaTime));
    }
}