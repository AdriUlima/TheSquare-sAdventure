using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using NaughtyAttributes;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Tiles/PlayerTile")]
public class PlayerTile : AnimatedTile
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        StartPos = Vector3Int.zero;
    }

    [SerializeField] private GameObject playerPrefab;
    [SerializeField, Scene] private string[] excludedScenes;
    public static Vector3Int StartPos; //todo
    bool IsLevelPlay(ITilemap tilemap) => tilemap.GetComponent<Tilemap>() != null && !excludedScenes.Contains(SceneManager.GetActiveScene().name) && Application.isPlaying;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        if (IsLevelPlay(tilemap))
        {
            tileData.sprite = null;
        }
    }

    public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
    {
        if (base.GetTileAnimationData(position, tilemap, ref tileAnimationData) == false) return false;
        if (IsLevelPlay(tilemap))
        {
            tileAnimationData.animatedSprites = null;
        }
        return true;
    }

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        if (IsLevelPlay(tilemap))
        {
            Instantiate(playerPrefab, tilemap.GetComponent<Tilemap>().GetCellCenterWorld(position), Quaternion.identity);
            StartPos = position;
        }
        return base.StartUp(position, tilemap, go);
    }

    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(PlayerTile))]
    public class PlayerTileEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script", "m_Transform", "m_InstancedGameObject");
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    #endregion
}