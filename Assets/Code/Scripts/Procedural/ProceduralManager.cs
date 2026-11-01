using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

#region Grid2D
public class Grid2D
{
    private readonly byte[,] grid;
    public readonly ushort Width, Height;
    private Vector2Int lastPos;

    public Grid2D(ushort w, ushort h)
    {
        Width = w;
        Height = h;
        grid = new byte[w,h];
    }

    public void SetCell(Vector2Int pos, byte cell)
    {
        grid[pos.x, pos.y] = cell;
        lastPos = pos;
    }
    public byte GetCell(Vector2Int pos, bool ignoreLastPos=false)
    {
        if (pos.x >= Width || pos.y >= Height) throw new Exception("Out of bounds");
        return (ignoreLastPos && pos == lastPos) ? (byte)0 : grid[pos.x, pos.y];
    }

    /*public void SwapColumns(ushort colA, ushort colB)
    {
        if (colA == colB) return;

        //(grid[colB], grid[colA]) = (grid[colA], grid[colB]);
    }

    public void SwapRows(ushort rowA, ushort rowB)
    {
        if (rowA == rowB) return;

        foreach (List<Cell> column in grid)
        {
            (column[rowB], column[rowA]) = (column[rowA], column[rowB]);
        }
    }*/

    /*public bool InsertRow(ushort index)
    {
        if (Height >= gridHeight.extraV) return false;

        foreach (List<Cell> column in grid)
            column.Insert(index, Cell.Null);

        return true;
    }

    public bool InsertColumn(ushort index)
    {
        if (Width >= gridWidth.extraV) return false;

        int h = Height;

        List<Cell> newCol = new(h);
        for (int y = 0; y < h; y++)
            newCol.Add(0);

        grid.Insert(index, newCol);
        return true;
    }*/

    public Vector2Int[] GetPosWithCell(byte b)
    {
        List<Vector2Int> result = new();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (grid[x, y] == b)
                    result.Add(new Vector2Int(x, y));
            }
        }

        return result.ToArray();
    }

    public Grid2D Clone()
    {
        Grid2D copy = new(Width, Height);
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                copy.SetCell(new Vector2Int(x, y), GetCell(new Vector2Int(x, y)));
        return copy;
    }
}
#endregion

public class ProceduralManager : MonoBehaviour
{
    //json nivel base a evolucionar
    //cuadrados obligatorios/prohibidos, cant mínima/máxima, cuadrado principal
    //[SerializeField, Range(0f, 1f)] private float barrierChance;
    //[SerializeField] private ushort barrierLength;
    [SerializeField, Expandable] private ProceduralGrid proceduralGrid;
    [SerializeField, Expandable] private ProceduralSolution proceduralSolution;
    [SerializeField, Expandable] private ProceduralSettings proceduralSettings;

    [Header("Tilemap Settings")]
    [SerializeField, Required] private Tilemap targetTilemap;
    [SerializeField] private TileBase[] tiles;

    [SerializeField, HideInInspector] private ushort stepAttempts = 10, maxAttempts = 100, nLevels = 5;
    private RandomProcedural random;

    void OnValidate()
    {
        stepAttempts = Math.Min(stepAttempts, maxAttempts);
    }

    public enum Movement : byte
    {
        Left, Right, Up, Down
    }
    
    private ushort w, h;
    private Func<bool>[] steps;
    private Vector2Int startPos, goalPos;
    private Grid2D grid;
    private HashSet<Vector2Int> stopPoints;
    int a;
    double startTime;
    private Movement[] solution;

    public void GenerateLevels()
    {
        startTime = Time.timeAsDouble;
        if (targetTilemap == null)
        {
            Debug.LogError("Falta asignar un Tilemap.");
            return;
        }
        random = new(proceduralSettings.random);
        steps = new Func<bool>[] { SetStartPos, SetTrack };
        for (int i = 0; i < nLevels; i++) GenerateLevel();
        double elapsed = Time.time - startTime;
        Debug.Log($"Total time: {elapsed} s.");
    }

    private void GenerateLevel()
    {
        SetParameters();

        int i = 0;
        a = 0;
    ExecuteStep:
        if (i < steps.Length) {
            for (int j = 0; j < stepAttempts; j++)
            {
                if (steps[i].Invoke())
                {
                    i++;
                    Debug.Log($"Step {i} completed!\t Attempts: {j + 1}");
                    goto ExecuteStep;
                }
                a++;
                if (a >= maxAttempts)
                {
                    Debug.LogError("MaxAttempts reached");
                    return;
                }
            }
            Debug.Log($"Step {i+1} attempts reached. Retrying Step {i}");
            i--;
            goto ExecuteStep;
        }
        LogResults();
    }

    private void SetParameters()
    {
        solution = proceduralSolution.solution;
        w = proceduralGrid.gridWidth.GetValueSetting();
        h = proceduralGrid.gridHeight.GetValueSetting();
    }

    private bool SetStartPos()
    {
        startPos = new(random.Range(0, w), random.Range(0, h));
        (startPos.x, startPos.y) = solution[0] switch
        {
            Movement.Left when startPos.x <= 1 => (2, startPos.y),
            Movement.Right when startPos.x >= w - 2 => (w-3, startPos.y),
            Movement.Up when startPos.y >= h - 2 => (startPos.x, h-3),
            Movement.Down when startPos.y <= 1 => (startPos.x, 2),
            _ => (startPos.x, startPos.y)
        };
        return true;
    }

    private bool SetTrack()
    {
        grid = new(w, h);
        int n = solution.Length;
        stopPoints = new(n);
        Vector2Int pos = startPos, lastStop = startPos;
        stopPoints.Add(pos);
        grid.SetCell(pos, 1);
        for (int i = 0; i < n; i++)
        {
            Movement m = solution[i];
            int maxD = GetMaxD(ref pos, m);
            if (maxD <= 0) return false;
            int distance = random.Range(0, maxD)+1;
            while (distance > 0)
            {
                if (IsNextEmpty(out _)) //Siguiente casilla disponible
                {
                    MoveNext();
                    distance--;
                    if (stopPoints.Contains(pos)) return false;
                    grid.SetCell(pos, 1);
                }
                else if (distance == maxD) return false;
                else break;
            }
            if (AnyStopNearby()) return false;
            if (i < n - 1)
            {
                stopPoints.Add(pos);
                lastStop = pos;
                if (IsNextEmpty(out Vector2Int p, restrictive: true)) grid.SetCell(p, 2);
                else return false;
            }

            void MoveNext() => pos = GetNext(ref pos, m);
            bool IsNextEmpty(out Vector2Int p, bool restrictive=false)
            {
                p = GetNext(ref pos, m);
                byte nextB = grid.GetCell(p);
                return nextB < (restrictive? 1 : 2);
            }
        }
        if (AnyStopNearby()) return false;
        goalPos = pos;
        return true;

        Vector2Int GetNext(ref Vector2Int p, Movement _m)
        {
            return p + _m switch
            {
                Movement.Left => Vector2Int.left,
                Movement.Right => Vector2Int.right,
                Movement.Up => Vector2Int.up,
                Movement.Down => Vector2Int.down,
                _ => throw new NotImplementedException()
            };
        }
        int GetMaxD(ref Vector2Int p, Movement _m)
        {
            return _m switch
            {
                Movement.Left => p.x - 1,
                Movement.Right => w - 2 - p.x,
                Movement.Up => h - 2 - p.y,
                Movement.Down => p.y - 1,
                _ => throw new NotImplementedException()
            };
        }
        bool AnyStopNearby()
        {
            if (stopPoints.Contains(pos)) return false;
            Movement[] all = { Movement.Left, Movement.Right, Movement.Up, Movement.Down };
            foreach (Movement mm in all)
            {
                Vector2Int aux = pos;
                int dist = GetMaxD(ref aux, mm);
                do
                {
                    aux = GetNext(ref aux, mm);
                    if (aux != lastStop && stopPoints.Contains(aux)) return true;
                    dist--;
                }
                while (dist > 0 && grid.GetCell(aux) != 2);
            }
            return false;
        }
    }

    private void LogResults()
    {
        string str = $"Level generated!\tTotal Attempts: {a}\nSolution: ";
        foreach (Movement m in solution) str += m + " ";
        Debug.Log(str);
        DrawTilemap();
    }

    private void DrawTilemap()
    {
        targetTilemap.ClearAllTiles();

        Vector3Int offset = new(-(grid.Width - 1) / 2, -(grid.Height - 1) / 2, 0);
        Vector2Int[] walls = grid.GetPosWithCell(2);
        foreach (Vector2Int w in walls)
        {
            targetTilemap.SetTile((Vector3Int)w + offset, tiles[2]);
        }
        /*Vector2Int[] empty = grid.GetPosWithCell(Cell.Empty);
        foreach (Vector2Int w in empty)
        {
            targetTilemap.SetTile((Vector3Int)w, playerTile);
        }*/

        targetTilemap.SetTile((Vector3Int)startPos + offset, tiles[0]);
        targetTilemap.SetTile((Vector3Int)goalPos + offset, tiles[1]);
    }

#region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(ProceduralManager))]
    public class ProceduralManager_New_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ProceduralManager _target = (ProceduralManager)target;

            EditorGUILayout.BeginHorizontal();
            SliderField(ref _target.stepAttempts, 1, 99, "StepAttempts");
            SliderField(ref _target.maxAttempts, 1, 999, "MaxAttempts");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Levels", new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold }, GUILayout.Width(150f))) _target.GenerateLevels();
            SliderField(ref _target.nLevels, 1, 100);
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                _target.OnValidate();
                EditorUtility.SetDirty(target);
            }

            void SliderField(ref ushort field, int min, int max, string label="")
            {
                if (label != string.Empty) EditorGUILayout.LabelField(label, GUILayout.Width(80f));
                field = (ushort)EditorGUILayout.IntSlider(field, min, max, GUILayout.Width(120f));
            }
        }
    }
#endif
    #endregion
}