using System.Collections.Generic;
using UnityEngine;

public class ItemNotificationManager : MonoBehaviour
{
    public static ItemNotificationManager Instance;
    public GameObject notificationPrefab;
    public Transform container;
    public int maxNotifications = 5;

    private List<GameObject> activeNotifications = new List<GameObject>();

    private void Awake() => Instance = this;

    private void OnEnable() => GameEvents.OnItemEarned += CreateItemNotification;
    private void OnDisable() => GameEvents.OnItemEarned -= CreateItemNotification;

    private void CreateItemNotification(ItemData data, int amount)
    {
        if (activeNotifications.Count >= maxNotifications)
        {
            GameObject oldest = activeNotifications[0];
            activeNotifications.RemoveAt(0);
            Destroy(oldest);
        }

        GameObject newNoti = Instantiate(notificationPrefab, container);
        activeNotifications.Add(newNoti);

        var script = newNoti.GetComponent<ItemNotificationSlot>();
        if (script != null)
        {
            script.SetData(data, amount);
        }
    }

    public void RemoveFromList(GameObject noti)
    {
        if (activeNotifications.Contains(noti))
        {
            activeNotifications.Remove(noti);
        }
    }
}