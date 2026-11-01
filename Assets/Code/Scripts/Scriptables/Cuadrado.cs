using UnityEngine;
using UnityEngine.Tilemaps;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Cuadrado", menuName = "Scriptable Objects/Cuadrado")]
public class Cuadrado : ScriptableObject
{
    public TileBase tile;
    public Interaccion interaccion;
    public Accion accion;
    public FinUsos finUsos;
    [HideIf(nameof(finUsos), FinUsos.Null)] public int nUsos;

    public enum Interaccion
    {
        Null,
        AlChocar,
        AlTraspasar,
        AlSalir
        //Blanco, Celeste, Morados
    }
    public enum Accion
    {
        Null,
        Detener,
        CambiarDireccion,
        Morir,
        Finalizar,
        Recolectarse,
        Transformarse, //Prefab
        Teletransportar,
        Empujar, //Vector2
        Impulsar
    }
    
    public enum FinUsos
    {
        Null,
        Desaparecer,
        Transformarse
    }
}