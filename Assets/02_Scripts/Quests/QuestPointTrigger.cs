using UnityEngine;

public enum DestinationID
{
    None,
    MyShop,

}


public class QuestPointTrigger : MonoBehaviour
{
    public DestinationID pointID;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameEvents.OnPointArrived?.Invoke(pointID);
        }
    }
}