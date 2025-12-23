using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    public Transform player;
    

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        WorldItem item = other.GetComponent<WorldItem>();
        if (item != null)
        {
            item.StartFollow(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        WorldItem item = other.GetComponent<WorldItem>();
        if (item != null)
        {
            item.StopFollow(player);
        }
    }
}
