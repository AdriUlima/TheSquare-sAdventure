using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Core : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        isLoading = false;
    }

    private bool NotNull; //static
    //[OdinSerialize, RequiredChild] private readonly GameObject gameplayOnly;
    [SerializeField] private GameManager gameManager;
    public static bool isLoading { get; private set; }

    void Awake()
    {
        if (NotNull == true) Destroy(gameObject);
        else
        {
            NotNull = true;
            isLoading = false;
            //DontDestroyOnLoad(gameObject);
            BehaviourPlus.SetGameManager(gameManager);
        }
    }

    public static void Win()
    {
        ExperimentManager.Instance.SubmitSessionLevel();
        isLoading = true;
        NextScene();
    }

    public static void ChangeScene(int buildIndex) => SceneManager.LoadScene(buildIndex);
    public static void ReloadScene() => ChangeScene(SceneManager.GetActiveScene().buildIndex);
    public static void NextScene() => ChangeScene(SceneManager.GetActiveScene().buildIndex+1);
    public static void PreviousScene() => ChangeScene(SceneManager.GetActiveScene().buildIndex-1);

    //public static void SetSelectedGO(GameObject go) => EventSystem.current.SetSelectedGameObject(go);

    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif !UNITY_WEBGL
        Application.Quit();
#else
        Debug.LogError("No compatible");
#endif
    }

    public static void Validate(Action onValidated, params (Func<bool> cond, string log, Action<string> debug)[] rules)
    {
        foreach (var (cond, log, debug) in rules)
        {
            if (cond())
            {
                debug(log);
                return;
            }
        }
        onValidated();
    }
}