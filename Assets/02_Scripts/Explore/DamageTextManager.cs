using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance { get; private set; }

    public GameObject damagePopupPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowDamage(float damageAmount, Vector3 position, bool isCritical = false)
    {
        if (damagePopupPrefab == null) return;

        // 텍스트 생성
        GameObject popup = Instantiate(damagePopupPrefab, position, Quaternion.identity);

        popup.transform.rotation = Camera.main.transform.rotation;

        DamagePopup popupScript = popup.GetComponent<DamagePopup>();
        if (popupScript != null)
        {
            popupScript.Setup(damageAmount, isCritical);
        }
    }
}