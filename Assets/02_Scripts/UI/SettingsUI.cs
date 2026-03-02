using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
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
        masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);

        SyncSliders();
    }

    private void OnMasterSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetMasterVolume(value);

        UpdateVolumeText(masterText, value);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    private void OnBGMSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetBGMVolume(value);

        UpdateVolumeText(bgmText, value);
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    private void OnSFXSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetSFXVolume(value);

        UpdateVolumeText(sfxText, value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
    private void UpdateVolumeText(TextMeshProUGUI textElement, float value)
    {
        if (textElement == null) return;

        int percentage = Mathf.RoundToInt(value * 100f);
        textElement.text = percentage.ToString() + "%";
    }

    private void SyncSliders()
    {
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1.0f);

        masterSlider.value = masterVol;
        bgmSlider.value = bgmVol;
        sfxSlider.value = sfxVol;

        UpdateVolumeText(masterText, masterVol);
        UpdateVolumeText(bgmText, bgmVol);
        UpdateVolumeText(sfxText, sfxVol);
    }
}