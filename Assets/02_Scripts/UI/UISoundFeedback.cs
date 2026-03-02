using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISoundFeedback : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    public AudioClip customHoverSound;
    public AudioClip customClickSound;

    [Range(0f, 1f)] public float volume = 1.0f;

    private AudioClip defaultHoverSound;
    private AudioClip defaultClickSound;

    private Selectable selectable;

    private void Start()
    {
        selectable = GetComponent<Selectable>();

        if (customHoverSound == null)
            defaultHoverSound = Resources.Load<AudioClip>("Sounds/DefaultHover");

        if (customClickSound == null)
            defaultClickSound = Resources.Load<AudioClip>("Sounds/DefaultClick");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectable != null && !selectable.interactable) return;

        AudioClip clipToPlay = customHoverSound != null ? customHoverSound : defaultHoverSound;
        PlaySound(clipToPlay);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable != null && !selectable.interactable)
        {
            return;
        }

        AudioClip clipToPlay = customClickSound != null ? customClickSound : defaultClickSound;
        PlaySound(clipToPlay);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && SoundManager.Instance != null)
        {
            Vector3 playPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            SoundManager.Instance.PlaySFX(clip, playPos, volume);
        }
    }
}