using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using NaughtyAttributes;
//using System.Threading.Tasks;
/*
public class JsonLevelPopup : EditorWindow
{
    private string fileName = "NuevoNivel.json";
    private static string confirmButton;
    private TaskCompletionSource<string> tcs;
string savePath = await JsonLevelPopup.LoadPathPopup();
    public static Task<string> SavePathPopup() => ShowPopup("Guardar JSON", "Guardar");

    public static Task<string> LoadPathPopup() => ShowPopup("Cargar JSON", "Cargar");

    private static Task<string> ShowPopup(string title, string cb)
    {
        JsonLevelPopup window = CreateInstance<JsonLevelPopup>();
        window.titleContent = new GUIContent(title);
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 100);
        confirmButton = cb;
        window.tcs = new();
        window.ShowUtility();
        return window.tcs.Task;
    }

    void OnGUI()
    {
        GUILayout.Label("Nombre del archivo:", EditorStyles.boldLabel);
        fileName = EditorGUILayout.TextField(fileName);
        GUILayout.Space(10);
        //GUILayout.BeginHorizontal();
        if (GUILayout.Button(confirmButton))
        {
            tcs.TrySetResult(Path.Combine(Application.dataPath, "Jsons", fileName));
            Close();
        }
        /*if (GUILayout.Button("Cancelar"))
        {
            tcs.TrySetResult(null);
            Close();
        }
        GUILayout.EndHorizontal();
    }
}*/


public class JsonManager : MonoBehaviour
{
    [SerializeField, RequiredChild] private Tilemap tilemap;
    [SerializeField, Required, FolderPath] private string folderPath;

    [Button]
    private void DeleteLevel()
    {
        tilemap.ClearAllTiles();
        Debug.Log("Nivel borrado del Tilemap.");
    }


    [Button]
    private void SaveIntoJson()
    {
#if UNITY_EDITOR
        string savePath = EditorUtility.SaveFilePanel("Guardar JSON", folderPath, "NuevoNivel", "json");
#else
        string savePath = "ToggleValue";
#endif
        if (string.IsNullOrEmpty(savePath))
        {
            Debug.Log("Cancelado.");
            return;
        }

        BoundsInt bounds = tilemap.cellBounds;
        Vector2Int min = new(int.MaxValue, int.MaxValue), max = new(int.MinValue, int.MinValue);
        foreach (Vector2Int pos in bounds.allPositionsWithin)
        {
            if (tilemap.GetTile((Vector3Int)pos) != null)
            {
                if (pos.x < min.x) min.x = pos.x;
                if (pos.y < min.y) min.y = pos.y;
                if (pos.x > max.x) max.x = pos.x;
                if (pos.y > max.y) max.y = pos.y;
            }
        }
        if (min.x == int.MaxValue)
        {
            Debug.LogWarning("Tilemap vacío, nada que guardar.");
            return;
        }
        Vector2Int size = new(max.x - min.x + 1, max.y - min.y + 1);
        JArray tilesArray = new();
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                TileBase tile = tilemap.GetTile(new Vector3Int(x, y));
                if (tile != null)
                {
                    Vector2Int rel = new(x - min.x, y - min.y);
                    tilesArray.Add(new JArray(rel.x, rel.y, tile.name)); // Estructura mínima: [x, y, tileName]
                }
            }
        }
        JObject level = new()
        {
            ["size"] = new JArray(size.x, size.y),
            ["tiles"] = tilesArray
        };
        
        Directory.CreateDirectory(Path.GetDirectoryName(savePath));
        File.WriteAllText(savePath, level.ToString(Formatting.None));
        Debug.Log($"Nivel guardado en {savePath} ({tilesArray.Count} celdas).");
    }

    [Button]
    private void LoadFromJson()
    {
#if UNITY_EDITOR
        string loadPath = EditorUtility.OpenFilePanelWithFilters("Cargar JSON", folderPath, new string[] { "Json files", "json"});
#else
        string loadPath = "ToggleValue";
#endif
        Core.Validate(ContinueLoad,
            (() => string.IsNullOrEmpty(loadPath), "Cancelado.", Debug.Log),
            (() => !File.Exists(loadPath), $"No se encontró el archivo: {loadPath}", Debug.LogWarning)
        );

        void ContinueLoad()
        {
            string json = File.ReadAllText(loadPath);
            JObject level = JObject.Parse(json);
            Vector2Int size = new((int)level["size"][0], (int)level["size"][1]);
            tilemap.ClearAllTiles();
            Vector2Int offset = new(-size.x / 2, -size.y / 2);
            foreach (JArray cell in level["tiles"])
            {
                int x = (int)cell[0], y = (int)cell[1];
                string tileName = (string)cell[2];
                TileBase tile = Resources.Load<TileBase>($"Tiles/{tileName}");
                if (tile != null) tilemap.SetTile(new Vector3Int(x + offset.x, y + offset.y, 0), tile);
                else Debug.LogWarning($"Tile no encontrado en Resources: {tileName}");
            }
            Debug.Log($"Nivel cargado y centrado ({size.x}x{size.y}).");
        }
    }
}