using UnityEngine;

[CreateAssetMenu(fileName = "ProceduralGrid", menuName = "Scriptable Objects/Procedural/Grid")]
public class ProceduralGrid : ScriptableObject
{
    [Header("Grid Settings")]
    public MinMaxSetting gridWidth = new("Width", 8, 3, 50);
    public MinMaxSetting gridHeight = new("Height", 8, 3, 50);

    void OnValidate()
    {
        gridWidth.Validate();
        gridHeight.Validate();
    }
}