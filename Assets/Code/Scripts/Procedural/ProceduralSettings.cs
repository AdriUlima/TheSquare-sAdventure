using UnityEngine;

[CreateAssetMenu(fileName = "ProceduralSettings", menuName = "Scriptable Objects/Procedural/Settings")]
public class ProceduralSettings : ScriptableObject
{
    [Header("Procedural Settings")]
    public RandomDistribution random;
    //[SerializeField, Range(0f, 1f)] private float noiseStrength = 0.1f;
    
}