using UnityEngine;

public class PlayButton : MonoBehaviour
{
    private bool isLoading;

    public void OnClick() => Core.NextScene();
    public void OnClose()
    {
        if (isLoading) return;
        isLoading = true;
        ExperimentManager.Instance.QuitExperiment();
    }
}