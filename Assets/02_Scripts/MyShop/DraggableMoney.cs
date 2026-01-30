using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;

public class DraggableMoney : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image image;
    private Transform dragLayerParent;
    private RectTransform myLimitZone;

    public int amount;
    public bool isPlayerMoney;
    public Action<DraggableMoney> OnMoneyDestroyed;

    private Vector3 startPosition;
    private bool isReturning = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Initialize(int amount, Sprite sprite, Vector2 definedSize, Transform dragLayer,
                           bool isMine, RectTransform limitZone, Action<DraggableMoney> onDestroyCallback)
    {
        this.amount = amount;
        this.isPlayerMoney = isMine;
        this.dragLayerParent = dragLayer;
        this.myLimitZone = limitZone;
        this.OnMoneyDestroyed = onDestroyCallback;

        if (image != null && sprite != null)
        {
            image.sprite = sprite;
            image.preserveAspect = true;
            if (definedSize != Vector2.zero) rectTransform.sizeDelta = definedSize;
            else image.SetNativeSize();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isReturning) return;
        if (transform.parent == dragLayerParent) return;

        startPosition = transform.position;

        // 드래그 레이어로 부모 변경하여 최상단 노출
        transform.SetParent(dragLayerParent);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isReturning) return;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isReturning) return;

        canvasGroup.blocksRaycasts = true;

        GameObject hitObject = eventData.pointerCurrentRaycast.gameObject;
        bool isValidDrop = false;

        if (hitObject != null)
        {
            if (hitObject == myLimitZone.gameObject || hitObject.transform.IsChildOf(myLimitZone))
            {
                isValidDrop = true;
            }
        }

        if (isValidDrop)
        {
            transform.SetParent(myLimitZone);
        }
        else
        {
            StartCoroutine(ReturnRoutine());
        }
    }

    private IEnumerator ReturnRoutine()
    {
        isReturning = true;

        Vector3 dropPosition = transform.position;
        float duration = 0.2f;
        float elapsed = 0f;


        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            transform.position = Vector3.Lerp(dropPosition, startPosition, t);
            yield return null;
        }

        transform.position = startPosition;
        transform.SetParent(myLimitZone);

        isReturning = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isReturning) return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (isPlayerMoney)
            {
                OnMoneyDestroyed?.Invoke(this);
                Destroy(gameObject);
            }
        }
    }
}