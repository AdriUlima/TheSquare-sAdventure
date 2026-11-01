using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Encuesta : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Elapsed = 0;
        Retries = 0;
    }

    private ExperimentManager experimentManager;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private RawImage img;
    [SerializeField] private ToggleGroup[] toggleGroups;
    [SerializeField] private TMP_InputField inputField;
    private string levelName;
    private int playOrder;
    public static double Elapsed;
    public static int Retries;

    void Start()
    {
        experimentManager = ExperimentManager.Instance;
        img.texture = experimentManager.renderCam;
        levelName = experimentManager.GetCurrentLevelData().Name;
        playOrder = experimentManager.CurrentIndex;
        text.text = $"Nivel {playOrder+1} / {experimentManager.TotalLevels}";
        Debug.Log($"{levelName} => Tiempo: {Elapsed}\t Retries: {Retries}");
    }

    public void OnClick()
    {
        if (toggleGroups.Any(g => !g.AnyTogglesOn()) || inputField.text.Equals(string.Empty)) return;

        Dictionary<string,FireSessionLevel> levels = experimentManager.FireSession.levels;
        levels.Add(levelName, new FireSessionLevel(5-ToggleValue(0), 5-ToggleValue(1), ToggleValue(2) == 0, inputField.text, playOrder, Elapsed, Retries));
        experimentManager.MoveNext();

        int ToggleValue(int i) => toggleGroups[i].GetFirstActiveToggle().transform.GetSiblingIndex();
    }
}