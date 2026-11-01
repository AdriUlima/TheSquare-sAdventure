using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class ParticipantData : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown[] dropdowns;
    [SerializeField] private Toggle toggle;
    [SerializeField] private TextMeshProUGUI uidText;

    async void Start()
    {
        await FirebaseManager.Initialized;
        uidText.text = FirebaseManager.Uid;
    }

    public void CopyGuid()
    {
        GUIUtility.systemCopyBuffer = uidText.text;
        Debug.Log($"Copied to clipboard: {uidText.text}");
    }

    public void OnClick()
    {
        if (dropdowns.Any(d => d.value == 0) || !toggle.isOn) return;
        ExperimentManager.Instance.SetParticipant(dropdowns.Select(d => d.value).ToArray());
        Core.NextScene();
    }
}