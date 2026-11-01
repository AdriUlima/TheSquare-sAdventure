using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = System.Random;

public class ExperimentManager : MonoBehaviour, IUpdatable
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    private readonly DateTime EndDate = new(2026, 7, 17, 5, 0, 0, DateTimeKind.Utc);
    public static ExperimentManager Instance { get; private set; }
    public RenderTexture renderCam;
    private FireParticipant participant;
    private FireConfig fireConfig;
    private readonly Random rng = new();
    private List<LevelData> SessionLevels;
    public int TotalLevels { get; private set; }
    public int CurrentIndex { get; private set; }
    public FireSession FireSession;

    async void Start()
    {
        //if (!IsActive()) EndExperiment();
        await FirebaseManager.InitializeAsync();
        if (FirebaseManager.FireConfig == null) EndExperiment();
        DontDestroyOnLoad(gameObject);
        Instance = this;
        fireConfig = FirebaseManager.FireConfig;

        static void EndExperiment()
        {
            Debug.Log("Experiment ended");
            Core.QuitGame();
        }
    }
    private bool IsActive() => DateTime.UtcNow < EndDate;

    public void SetParticipant(int[] values) => participant = new(values[0], values[1], values[2]);

    public Task SessionReady { get; private set; }
    public Task CreateSessionAsync() => SessionReady ??= CreateSessionInternal();
    private async Task CreateSessionInternal()
    {
        FireLevels fireLevels = await FirebaseManager.GetLevels();
        Subfolders.Initialize(fireConfig, fireLevels);
        PickSessionLevels();
        await LoadJsons();
        TotalLevels = SessionLevels.Count;
        CurrentIndex = 0;

        void PickSessionLevels()
        {
            SessionLevels = new();
            foreach (var sub in Subfolders.All)
            {
                SessionLevels.AddRange(PickByAmount(FilterByMargin(sub.Levels), sub.Amount));
            }

            ShuffleLevels();
        }

        List<LevelData> FilterByMargin(Dictionary<string,FireLevel> levels)
        {
            int min = levels.Values.Min(l => l.usageCount);

            return levels.Where(l => l.Value.usageCount <= min + fireConfig.balanceMargin)
                         .Select(l => new LevelData(l.Key)).ToList();
        }

        HashSet<LevelData> PickByAmount(List<LevelData> pool, int amount)
        {
            HashSet<LevelData> selected = new(amount);
            for (int i = 0; i < amount; i++)
            {
                int n = rng.Next(pool.Count);
                selected.Add(pool[n]);
                pool.RemoveAt(n);
            }
            return selected;
        }

        void ShuffleLevels()
        {
            for (int i = SessionLevels.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (SessionLevels[i], SessionLevels[j]) = (SessionLevels[j], SessionLevels[i]);
            }
        }

        async Task LoadJsons()
        {
            for (int i = 0; i < SessionLevels.Count; i++)
            {
                LevelData levelData = SessionLevels[i];
                AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(levelData.Name);
                TextAsset jsonAsset = await handle.Task;
                levelData.Json = jsonAsset.text;
                Addressables.Release(handle);
            }
        }
    }

    public async void StartExperiment()
    {
        await SessionReady;
        FireSession = new(FirebaseManager.Uid);
        CurrentIndex = 0;
        n = -1;
        Core.NextScene();
    }

    public LevelData GetCurrentLevelData() => SessionLevels[CurrentIndex];

    public void TakeScreenshot()
    {
        Camera cam = Camera.main;
        cam.targetTexture = renderCam;
        cam.Render();
        cam.targetTexture = null;
    }

    public void MoveNext()
    {
        CurrentIndex++;
        if (CurrentIndex < SessionLevels.Count) Core.PreviousScene();
        else Core.NextScene();
    }

    public void CompleteExperiment() => FirebaseManager.UploadParticipantAsync(participant, FireSession);

    public async void QuitExperiment()
    {
        await FirebaseManager.ParticipantUploaded;
        Core.QuitGame();
    }

    private int n;
    private float timer;
    public void OnUpdate() => timer += Time.deltaTime;
    public void PauseSession() => UpdateManager.UnregisterUpdate(this);
    public void UnpauseSession()
    {
        if (n != CurrentIndex) StartNew();
        else Resume();
        UpdateManager.RegisterUpdate(this);

        void StartNew()
        {
            n = CurrentIndex;
            Encuesta.Retries = 0;
            timer = 0f;
        }
        void Resume()
        {
            Encuesta.Retries++;
        }
    }
    public void SubmitSessionLevel()
    {
        PauseSession();
        Encuesta.Elapsed = timer;
    }
}

[Serializable]
public class LevelData
{
    public string Name, Json;

    public LevelData(string name)
    {
        Name = name;
    }
}