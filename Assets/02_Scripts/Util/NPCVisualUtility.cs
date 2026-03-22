using UnityEngine;

public static class NPCVisualUtility
{
    public static void ApplyRandomColor(GameObject npcObject)
    {
        Color pastelColor = Random.ColorHSV(0f, 1f, 0.25f, 0.45f, 0.9f, 1.0f);

        ColorChanger colorChanger = npcObject.GetComponentInChildren<ColorChanger>();
        if (colorChanger != null)
        {
            colorChanger.SetDynamicColor(pastelColor);
        }
    }
}