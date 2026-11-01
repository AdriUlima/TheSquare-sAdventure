using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using static BehaviourPlus;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class CapTile
{
    private readonly List<Vector3Int> queue = new();
    [SerializeField, HideInInspector] private TextMeshProUGUI text;
    public int Cap;
    public int Remaining => Cap - queue.Count;

    public void OnLoad()
    {
        UpdateButtonText();
    }

    public void Enqueue(Vector3Int item)
    {
        queue.Add(item);
        UpdateButtonText();
    }
    public Vector3Int Dequeue()
    {
        if (queue.Count == 0) throw new InvalidOperationException("La cola está vacía");

        Vector3Int item = queue[0];
        queue.RemoveAt(0);
        UpdateButtonText();
        return item;
    }
    public bool Remove(Vector3Int item)
    {
        bool b = queue.Remove(item);
        UpdateButtonText();
        return b;
    }

    public void SetTextMeshPro(TextMeshProUGUI t)
    {
        text = t;
        UpdateButtonText();
    }
    public void UpdateButtonText() => text.text = Remaining.ToString();
}

public class LevelEditorManager : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    public static LevelEditorManager Instance { get; private set; }

    private Camera cam;
    [Header("Tilemaps")]
    [SerializeField] private Tilemap editorMap;
    [SerializeField] private Tilemap previewMap;
    [Header("Settings")]
    [SerializeField] private Texture2D eraserIcon;
    private bool isErasing;
    [SerializeField] private TileBase[] tiles;
    [SerializeField] private Dictionary<TileBase, CapTile> capTiles;
    private Vector3Int currentPos;
    private TileBase currentTile;
    [Header("Movement")]
    [SerializeField] private float dragSpeed = 1f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField, MinMaxSlider(1f, 30f)] private Vector2 zoomClamp;
    private bool isCameraMoving;
    [SerializeField] private RectTransform panelGO;
    [SerializeField] private TileButton tileButtonPrefab;


#if UNITY_EDITOR
    [Button("Load TileButtons")]
    private void LoadTileButtons()
    {
        for (int i = panelGO.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(panelGO.GetChild(i).gameObject);
        }
        foreach (TileBase t in tiles)
        {
            TileButton tButton = (TileButton)InstantiateButton();
            tButton.SetTileBase(t);
            if (capTiles.TryGetValue(t, out CapTile c))
            {
                c.SetTextMeshPro(tButton.ActivateTextMeshPro());
            }
        }
        InstantiateButton();
        
        UnityEngine.Object InstantiateButton() => PrefabUtility.InstantiatePrefab(tileButtonPrefab, panelGO);
    }
#endif

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    public void OnLoad()
    {
        foreach (CapTile t in capTiles.Values) t.OnLoad();
    }

    void Update()
    {
        if (inputHandler.DiscardTile)
        {
            currentTile = null;
            if (isErasing) DeactivateEraser();
            previewMap.SetTile(currentPos, null);
        }

        float camZoom = inputHandler.CameraZoom;
        if (camZoom != 0f)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - camZoom * zoomSpeed, zoomClamp.x, zoomClamp.y);
        }

        if (isCameraMoving)
        {
            Vector2 camMov = inputHandler.CameraMov.Value;
            if (inputHandler.CameraMov.IsTriggered)
            {
                cam.transform.Translate(dragSpeed * camMov);
            }
            else isCameraMoving = false;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            previewMap.SetTile(currentPos, null);
            return;
        }

        if (inputHandler.MousePos.TryGetValue(out Vector2 mousePos))
        {
            Vector3Int newPos = previewMap.WorldToCell(cam.ScreenToWorldPoint(mousePos));
            if (isErasing)
            {
                if (inputHandler.PlaceTile.IsTriggered) SetTile(newPos, null);
            }
            else
            {
                bool posChanged = newPos != currentPos;
                if (posChanged)
                {
                    previewMap.SetTile(currentPos, null);
                    currentPos = newPos;
                }
                if (inputHandler.PlaceTile.IsStarted || (posChanged && inputHandler.PlaceTile.IsTriggered))
                {
                    if (currentTile != null) SetTile(newPos, currentTile);
                }
                else if (posChanged)
                {
                    previewMap.SetTile(newPos, currentTile);
                }
            }
        }

        if (isCameraMoving == false && inputHandler.CameraMov.IsStarted)
        {
            isCameraMoving = true;
        }
    }

    private void SetTile(Vector3Int pos, TileBase tile)
    {
        TileBase oldTile = editorMap.GetTile(pos);
        if (oldTile == tile) return;
        else if (oldTile != null && capTiles.TryGetValue(oldTile, out CapTile oldInfo))
        {
            oldInfo.Remove(pos);
        }
        if (tile == null)
        {
            editorMap.SetTile(pos, null);
            return;
        }
        if (capTiles.TryGetValue(tile, out CapTile info))
        {
            info.Enqueue(pos);
            if (info.Remaining < 0)
            {
                editorMap.SetTile(info.Dequeue(), null);
            }
        }
        previewMap.SetTile(pos, null);
        editorMap.SetTile(pos, tile);
    }

    public void SetCurrentTile(TileBase t)
    {
        currentTile = t;
        if (t == null) ActivateEraser();
        else if (isErasing) DeactivateEraser();
    }

    public void ActivateEraser()
    {
        isErasing = true;
        Cursor.SetCursor(eraserIcon, Vector2.zero, CursorMode.Auto);
    }

    private void DeactivateEraser()
    {
        isErasing = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}