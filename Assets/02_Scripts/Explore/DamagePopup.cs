using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private Color textColor;

    private float moveYSpeed = 3f;
    private float disappearTimer = 0.5f;
    private float fadeSpeed = 5f;

    private float initialScale = 0.5f;
    private float targetScale = 1.5f;
    private float currentScale;

    void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshPro>();

        transform.localScale = Vector3.one * initialScale;
        currentScale = initialScale;
    }
    void Update()
    {
        transform.position += Vector3.up * moveYSpeed * Time.deltaTime;

        if (disappearTimer > 0)
        {
            currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * 10f);
            transform.localScale = Vector3.one * currentScale;
        }

        disappearTimer -= Time.deltaTime;

        if (disappearTimer < 0)
        {
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Setup(float damageAmount, bool isCritical)
    {
        textMesh.text = damageAmount.ToString("0");

        if (isCritical)
        {
            textMesh.fontSize = 5;
            textMesh.color = new Color(1f, 0.2f, 0.2f); // Ä¡¸íÅ¸ »¡°­
        }
        else
        {
            textMesh.fontSize = 4;
            textMesh.color = new Color(1f, 0.9f, 0.2f); // ÀÏ¹Ý ³ë¶û
        }

        textColor = textMesh.color;
        disappearTimer = 0.5f;
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}