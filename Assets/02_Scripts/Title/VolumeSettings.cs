using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [Header("Volume")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public TextMeshProUGUI masterText;
    public TextMeshProUGUI bgmText;
    public TextMeshProUGUI sfxText;

    private void Start()
    {
        SyncSliders();

        masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }

    private void OnMasterSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetMasterVolume(value);

        UpdateVolumeText(masterText, value);
        PlayerPrefs.SetFloat(SettingsConstants.PREF_MASTER_VOL, value);
        PlayerPrefs.Save();
    }

    private void OnBGMSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetBGMVolume(value);

        UpdateVolumeText(bgmText, value);
        PlayerPrefs.SetFloat(SettingsConstants.PREF_BGM_VOL, value);
        PlayerPrefs.Save();
    }

    private void OnSFXSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetSFXVolume(value);

        UpdateVolumeText(sfxText, value);
        PlayerPrefs.SetFloat(SettingsConstants.PREF_SFX_VOL, value);
        PlayerPrefs.Save();
    }

    private void UpdateVolumeText(TextMeshProUGUI textElement, float value)
    {
        if (textElement == null) return;

        int percentage = Mathf.RoundToInt(value * 100f);
        textElement.text = percentage.ToString() + "%";
    }

    private void SyncSliders()
    {
        // 상수(SettingsConstants) 사용
        float masterVol = PlayerPrefs.GetFloat(SettingsConstants.PREF_MASTER_VOL, 1.0f);
        float bgmVol = PlayerPrefs.GetFloat(SettingsConstants.PREF_BGM_VOL, 1.0f);
        float sfxVol = PlayerPrefs.GetFloat(SettingsConstants.PREF_SFX_VOL, 1.0f);

        masterSlider.SetValueWithoutNotify(masterVol);
        bgmSlider.SetValueWithoutNotify(bgmVol);
        sfxSlider.SetValueWithoutNotify(sfxVol);

        UpdateVolumeText(masterText, masterVol);
        UpdateVolumeText(bgmText, bgmVol);
        UpdateVolumeText(sfxText, sfxVol);
    }
}