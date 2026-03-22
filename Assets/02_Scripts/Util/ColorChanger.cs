using UnityEngine;

[ExecuteAlways]
public class ColorChanger : MonoBehaviour
{
    public Color myColor = Color.white;
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    private bool _isColorOverridden = false;

    private void OnValidate()
    {
        if (!Application.isPlaying) ApplyColor(myColor);
    }

    private void Start()
    {
        if (!_isColorOverridden)
        {
            ApplyColor(myColor);
        }
    }

    public void SetDynamicColor(Color newColor)
    {
        _isColorOverridden = true;
        myColor = newColor;
        ApplyColor(newColor);
    }

    private void ApplyColor(Color targetColor)
    {
        if (_renderer == null) _renderer = GetComponentInChildren<Renderer>();
        if (_renderer == null) return;

        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        _renderer.GetPropertyBlock(_propBlock, 0);
        _propBlock.SetColor("_Color", targetColor);
        _renderer.SetPropertyBlock(_propBlock, 0);
    }
}