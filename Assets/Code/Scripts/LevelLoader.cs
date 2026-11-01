using UnityEngine;
using UnityEngine.Tilemaps;
using Newtonsoft.Json.Linq;
using System.Linq;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;

    void Awake() => ExperimentManager.Instance.PauseSession();

    void Start()
    {
        tilemap.ClearAllTiles();
        ExperimentManager experimentManager = ExperimentManager.Instance;

        string json = experimentManager.GetCurrentLevelData().Json;
        JObject level = JObject.Parse(json);
        Vector2Int size = new((int)level["size"][0], (int)level["size"][1]);
        Vector2Int offset = size / -2;
        foreach (JArray cell in level["tiles"].Cast<JArray>())
        {
            int x = (int)cell[0], y = (int)cell[1];
            string tileName = (string)cell[2];
            TileBase tile = Resources.Load<TileBase>($"Tiles/{tileName}");
            if (tile != null) tilemap.SetTile(new Vector3Int(x + offset.x, y + offset.y), tile);
            else Debug.LogWarning($"Tile no encontrado en Resources: {tileName}");
        }
        Debug.Log($"Nivel cargado y centrado ({size.x}x{size.y}).");
        experimentManager.TakeScreenshot();
    }
}