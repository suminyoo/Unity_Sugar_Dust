using UnityEngine;

public class LightController : MonoBehaviour
{
    [Header("Target Light")]
    [SerializeField] private Light directionalLight;

    [Header("Intensity Settings")]
    [SerializeField] private float morningIntensity = 0.8f;
    [SerializeField] private float dayIntensity = 1.0f;
    [SerializeField] private float eveningIntensity = 0.5f;
    [SerializeField] private float nightIntensity = 0.1f;

    [Header("Color Settings")]
    [SerializeField] private Color morningColor = new Color(1f, 0.9f, 0.7f);
    [SerializeField] private Color dayColor = Color.white;
    [SerializeField] private Color eveningColor = new Color(1f, 0.5f, 0.3f);
    [SerializeField] private Color nightColor = new Color(0.2f, 0.3f, 0.6f);

    private bool _isIndoor = false;

    private void Awake()
    {
        if (directionalLight == null)
        {
            FindDirectionalLight();
        }
    }

    private void OnEnable()
    {
        GameManager.OnTimeChanged += HandleTimeChanged;
    }

    private void OnDisable()
    {
        GameManager.OnTimeChanged -= HandleTimeChanged;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            UpdateLight(GameManager.Instance.currentTime);
        }
    }

    private void FindDirectionalLight()
    {
        Light[] lights = GameObject.FindObjectsOfType<Light>();
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional)
            {
                directionalLight = l;
                break;
            }
        }
    }

    private void HandleTimeChanged(GAME_TIME newTime, bool isInstant)
    {
        UpdateLight(newTime);
    }


    public void SetIndoorMode(bool indoor)
    {
        _isIndoor = indoor;

        if (GameManager.Instance != null)
            UpdateLight(GameManager.Instance.currentTime);
    }

    private void UpdateLight(GAME_TIME time)
    {
        if (directionalLight == null) return;

        // 실내
        if (_isIndoor)
        {
            directionalLight.enabled = false;
            return;
        }

        directionalLight.enabled = true;

        // 시간대
        switch (time)
        {
            case GAME_TIME.Morning:
                SetLightState(morningIntensity, morningColor);
                break;
            case GAME_TIME.Day:
                SetLightState(dayIntensity, dayColor);
                break;
            case GAME_TIME.Evening:
                SetLightState(eveningIntensity, eveningColor);
                break;
            case GAME_TIME.Night:
                SetLightState(nightIntensity, nightColor);
                break;
        }
    }

    private void SetLightState(float intensity, Color color)
    {
        directionalLight.intensity = intensity;
        directionalLight.color = color;
    }
}