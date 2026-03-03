using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISoundFeedback : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    public SoundData customHoverSound;
    public SoundData customClickSound;

    private float defaultVolume = 0.2f;
    private AudioClip defaultHoverSound;
    private AudioClip defaultClickSound;
    private Selectable selectable;

    private void Start()
    {
        selectable = GetComponent<Selectable>();

        if (customHoverSound.clip == null)
            defaultHoverSound = Resources.Load<AudioClip>("Sounds/DefaultHover");

        if (customClickSound.clip == null)
            defaultClickSound = Resources.Load<AudioClip>("Sounds/DefaultClick");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectable != null && !selectable.interactable) return;

        SoundData dataToPlay;

        if (customHoverSound.clip != null)
        {
            dataToPlay = customHoverSound;
        }
        else if (defaultHoverSound != null)
        {
            dataToPlay = new SoundData { clip = defaultHoverSound, volume = defaultVolume };
        }
        else return;

        PlaySound(dataToPlay);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable != null && !selectable.interactable) return;
        
        SoundData dataToPlay;

        if (customClickSound.clip != null)
        {
            dataToPlay = customClickSound;
        }
        else if (defaultClickSound != null)
        {
            dataToPlay = new SoundData { clip = defaultClickSound, volume = defaultVolume };
        }
        else return;
        PlaySound(dataToPlay);
    }

    private void PlaySound(SoundData data)
    {
        if (data.clip != null && SoundManager.Instance != null)
        {
            Vector3 playPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            SoundManager.Instance.PlaySFX2D(data);
        }
    }
}