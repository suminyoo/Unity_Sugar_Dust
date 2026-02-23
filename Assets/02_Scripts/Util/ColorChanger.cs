using UnityEngine;

[ExecuteAlways]
public class ColorChanger : MonoBehaviour
{
    public Color myColor = Color.white;
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    private void OnValidate()
    {
        ApplyColor();
    }

    private void Start()
    {
        ApplyColor();
    }

    private void ApplyColor()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor("_Color", myColor);
        _renderer.SetPropertyBlock(_propBlock);
    }
}