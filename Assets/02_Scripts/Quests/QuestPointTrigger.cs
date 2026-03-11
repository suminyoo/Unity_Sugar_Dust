using UnityEngine;

public enum PointID
{
    None,
    MyShop,

}


public class QuestPointTrigger : MonoBehaviour
{
    public PointID pointID;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameEvents.OnPointArrived?.Invoke(pointID);
        }
    }
}