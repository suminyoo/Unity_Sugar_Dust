using UnityEngine;

public class Mineral : MonoBehaviour, IMineable
{
    public MineralData data;
    public HealthBar healthBar;

    private float currentHealth;

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        currentHealth = data.maxHealth;
        healthBar.UpdateHealth(currentHealth, data.maxHealth);
    }

    public void OnMine(float power, bool isCritical)
    {
        if (data == null) return;

        currentHealth -= power;

        DamageTextManager.Instance.ShowDamage(power, transform.position + Vector3.up, isCritical);
        healthBar.UpdateHealth(currentHealth, data.maxHealth);
        
        if (data.hitEffectPrefab != null) Instantiate(data.hitEffectPrefab, transform.position, Quaternion.identity);
        if (data.mineSound != null) SoundManager.Instance.PlaySFX(data.mineSound, transform.position);
        

        GetComponent<HitEffect>()?.PlayHitFlash();

        if (currentHealth <= 0)
        {
            DestoryMineral();
        }
    }

    void DestoryMineral()
    {
        if (data.lootTable != null) data.lootTable.SpawnItem(transform.position);

        Destroy(gameObject);
    }
}