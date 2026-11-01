using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class RemoveSingletonsFromScenes// : IProcessSceneWithReport
{
    /*public int callbackOrder => 0;

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        if (!BuildPipeline.isBuildingPlayer || scene.name == "Bootstrap") return;
        GameManager gm = Object.FindAnyObjectByType<GameManager>();
        if (gm == null) return;

        Object.DestroyImmediate(gm.gameObject);
    }*/
}
#endif