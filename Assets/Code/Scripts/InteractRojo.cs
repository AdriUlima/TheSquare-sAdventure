#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class InteractRojo : MonoBehaviour
{
    public enum Interaccion
    {
        Null,
        AlChocar,
        AlTraspasar,
        AlSalir
        //Blanco, Celeste, Morados
    }
    [SerializeField] public Interaccion interaccion;
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
    [SerializeField] public Accion accion;
    private int nUsos;
    public enum FinUsos
    {
        Null,
        Desaparecer,
        Transformarse
    }
    [SerializeField] private FinUsos finUsos;

    void OnValidate()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.hideFlags = HideFlags.HideInInspector;
        switch (interaccion)
        {
            case Interaccion.AlChocar:
                col.enabled = true;
                col.isTrigger = false;
                break;
            case Interaccion.AlTraspasar:
            case Interaccion.AlSalir:
                col.enabled = true;
                col.isTrigger = true;
                break;
            default:
                col.enabled = false;
                break;
        }
    }
    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(InteractRojo))]
    public class InteractRojoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            InteractRojo _target = (InteractRojo)target;
            if (_target.finUsos != FinUsos.Null)
            {
                _target.nUsos = EditorGUILayout.IntField(_target.nUsos, GUILayout.Width(50));
            }
        }
    }
#endif
#endregion
}