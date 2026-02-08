using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI Reference")]
    public Image healthFillImage;
    public GameObject uiCanvas; 

    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        // ºôº¸µå
        transform.LookAt(transform.position + cam.forward);
    }

    public void UpdateHealth(float currentHp, float maxHp)
    {
        if (uiCanvas == null || healthFillImage == null) return;

        healthFillImage.fillAmount = currentHp / maxHp;

        if (currentHp >= maxHp || currentHp <= 0)
        {
            if (uiCanvas.activeSelf) uiCanvas.SetActive(false);
        }
        else
        {
            if (!uiCanvas.activeSelf) uiCanvas.SetActive(true);
        }
    }
}