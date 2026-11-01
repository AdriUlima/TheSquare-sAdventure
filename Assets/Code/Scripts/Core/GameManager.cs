using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using static BehaviourPlus;

public class GameManager : MonoBehaviour_UU, IUpdatable
{
    private int rojosN, rojosRem;
    public event Action<Vector2Int> MoveRojo;
    public Cuadrado[] cuadrados;
    public Tilemap tilemap;

    #region OLD
    private int cFalsoFaltantes;
    private Dictionary<Vector2Int, Vector2> Teleports = new();
    public GameObject prefabNegro;
    public event Action UpdateVerde;
    public bool invertVerde { get; private set; }
    #endregion

    void Awake()
    {
        SceneManager.sceneLoaded += SceneLoaded;

        #region OLD
        //cFalsoFaltantes = GameObject.FindGameObjectsWithTag("CelesteFalso").Length;
        //if (cFalsoFaltantes > 0) GameObject.FindGameObjectWithTag("Celeste").SetActive(false);
        #endregion
    }
    void OnDestroy() => SceneManager.sceneLoaded -= SceneLoaded;

    private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        rojosN = rojosRem = 0;
    }
    public void RegisterRojo() => rojosN++;

    public void OnUpdate()
    {
        if (rojosRem == 0 && inputHandler.MoveDir != Vector2.zero)
        {
            rojosRem = rojosN;
            MoveRojo(inputHandler.MoveDir);
        }
    }
    public void RojoQuieto() => rojosRem--;

    #region OLD
    public int AddTeleport(Vector2 m1, Vector2 m2)
    {
        int aux = Teleports.Count / 2;
        Teleports.Add(new Vector2Int(aux, 1), m1);
        Teleports.Add(new Vector2Int(aux, 2), m2);
        return aux;
    }

    public Vector2 GetTeleportNewPos(string name)
    {
        return Teleports[new Vector2Int(int.Parse(name[0].ToString()), name[1] == '1' ? 2 : 1)];
    }

    public void CelesteFalsoCollected()
    {
        cFalsoFaltantes--;
        if (cFalsoFaltantes == 0) GameObject.FindGameObjectWithTag("Celeste").SetActive(true);
    }

    public void InvertVerde()
    {
        invertVerde = !invertVerde;
        UpdateVerde();
    }
    #endregion
}