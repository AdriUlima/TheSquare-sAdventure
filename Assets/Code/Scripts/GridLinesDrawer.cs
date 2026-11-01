using UnityEngine;
using UnityEngine.Rendering;

public class GridlinesDrawer : MonoBehaviour
{
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Grid grid;
    private Camera cam;
    private int linesX, linesY;
    private float startX, startY, cellSize, cellGap;

    void Start()
    {
        cam = Camera.main;
        //SetGrid();
    }
    void OnEnable() => RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    void OnDisable() => RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;

    void OnEndCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        if (cam == this.cam) DrawGrid();
    }

    void SetGrid()
    {
        Vector2 camPos = cam.transform.position;
        float camMidHeight = cam.orthographicSize, camMidWidth = camMidHeight * cam.aspect;
        // punto inicial (abajo izquierda)
        cellGap = grid.cellGap.x;
        cellSize = grid.cellSize.x + cellGap;
        startX = Mathf.Floor((camPos.x - camMidWidth) / cellSize) * cellSize - grid.cellGap.x;
        startY = Mathf.Floor((camPos.y - camMidHeight) / cellSize) * cellSize - grid.cellGap.y;
        linesX = Mathf.CeilToInt(camMidWidth * 2 / cellSize) + 2;
        linesY = Mathf.CeilToInt(camMidHeight * 2 / cellSize) + 2;
    }

    void DrawGrid()
    {
        SetGrid();
        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity);
        lineMaterial.SetPass(0);
        GL.Begin(GL.QUADS);

        float t = startX, tot = startY + linesY * cellSize;
        for (int i = 0; i < linesX; i++)
        {
            DrawLineWithThickness(t, startY, t + cellGap, tot);
            t += cellSize;
        }
        t = startY;
        tot = startX + linesX * cellSize;
        for (int i = 0; i < linesY; i++)
        {
            DrawLineWithThickness(startX, t, tot, t + cellGap);
            t += cellSize;
        }

        GL.End();
        GL.PopMatrix();
    }

    private void DrawLineWithThickness(float x1, float y1, float x2, float y2)
    {
        float z = 191f;
        GL.Vertex(new Vector3(x1, y1, z));
        GL.Vertex(new Vector3(x1, y2, z));
        GL.Vertex(new Vector3(x2, y2, z));
        GL.Vertex(new Vector3(x2, y1, z));
    }
}