using UnityEngine;

public static class NPCVisualUtility
{
    private static MaterialPropertyBlock _propBlock;

    public static void ApplyRandomColor(GameObject npcObject, int bodyMaterialIndex = 0)
    {
        var renderer = npcObject.GetComponentInChildren<SkinnedMeshRenderer>();
        if (renderer == null) return;

        if (_propBlock == null)
            _propBlock = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(_propBlock, bodyMaterialIndex);

        Color pastelColor = Random.ColorHSV(0f, 1f, 0.25f, 0.45f, 0.9f, 1.0f);

        _propBlock.SetColor("_Color", pastelColor);
        renderer.SetPropertyBlock(_propBlock, bodyMaterialIndex);
    }
}