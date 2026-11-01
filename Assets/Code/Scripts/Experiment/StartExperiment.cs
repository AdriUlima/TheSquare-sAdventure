using UnityEngine;
using TMPro;

public class StartExperiment : MonoBehaviour
{
    private ExperimentManager experimentManager;
    [SerializeField] private TextMeshProUGUI totalNiveles;
    private bool isLoading;

    void Start()
    {
        experimentManager = ExperimentManager.Instance;
        experimentManager.CreateSessionAsync();
        FireConfig fireConfig = FirebaseManager.FireConfig;
        totalNiveles.text = $"{fireConfig.humanLevels+fireConfig.proceduralLevels} niveles";
    }

    public void OnStartExperiment()
    {
        if (isLoading) return;
        isLoading = true;
        experimentManager.StartExperiment();
    }
}