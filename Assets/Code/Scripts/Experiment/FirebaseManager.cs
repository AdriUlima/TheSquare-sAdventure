using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public static class FirebaseManager
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Initialized = null;
        dataset = null;
        FireConfig = null;
        Uid = null;
        ParticipantUploaded = null;
    }

    private const string config = "config", experiment = "experiment", data = "data", levels = "levels", participants = "participants", sessions = "sessions";
    private static FirebaseFirestore dataset;
    public static FireConfig FireConfig { get; private set; }
    public static string Uid { get; private set; }

    public static Task Initialized { get; private set; }
    public static Task InitializeAsync() => Initialized ??= InitializeInternal();

    private static async Task InitializeInternal()
    {
        await MainInitialization();
        FireConfig = await GetConfig();
        CreateUid();
        Debug.Log("Firebase initialized.");

        static async Task MainInitialization()
        {
            DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (status != DependencyStatus.Available) throw new Exception($"Firebase dependencies not available: {status}");

#if UNITY_EDITOR
            if (FirebaseApp.DefaultInstance == null) FirebaseApp.Create();
#endif
            dataset = FirebaseFirestore.DefaultInstance;
        }

        static async Task<FireConfig> GetConfig()
        {
            DocumentReference experimentRef;
            try { experimentRef = dataset.Collection(config).Document(experiment); }
            catch (FirestoreException e)
            {
                Debug.LogError($"GetConfig failed: {e.Message}");
                return null;
            }
            DocumentSnapshot snapshot = await experimentRef.GetSnapshotAsync();
            if (!snapshot.Exists) throw new Exception("Config document not found.");
            return snapshot.ConvertTo<FireConfig>();
        }

        static void CreateUid() => Uid = dataset.Collection(participants).Document().Id;
    }

    public static async Task<FireLevels> GetLevels()
    {
        FireLevels fireLevels = new();

        DocumentReference levelsRef = dataset.Collection(data).Document(levels);
        for (int i = 0; i < Subfolders.Names.Length; i++)
        {
            QuerySnapshot snapshot = await levelsRef.Collection(Subfolders.Names[i]).GetSnapshotAsync();
            var aux = snapshot.Documents.ToDictionary(d => d.Id, d => d.ConvertTo<FireLevel>());
            fireLevels = Subfolders.InsertDict(fireLevels, aux, i);
        }
        return fireLevels;
    }

    public static Task ParticipantUploaded { get; private set; }
    public static Task UploadParticipantAsync(FireParticipant fireParticipant, FireSession fireSession) => ParticipantUploaded ??= UploadInternal(fireParticipant, fireSession);
    private async static Task UploadInternal(FireParticipant fireParticipant, FireSession fireSession)
    {
        WriteBatch batch = dataset.StartBatch();
        DocumentReference participantRef = dataset.Collection(participants).Document(Uid);
        batch.Set(participantRef, fireParticipant);
        DocumentReference sessionRef = dataset.Collection(sessions).Document();
        batch.Set(sessionRef, fireSession);
        DocumentReference levelsRef = dataset.Collection(data).Document(levels);
        foreach (var l in fireSession.levels)
        {
            batch.Set(sessionRef.Collection(levels).Document(l.Key), l.Value);
            batch.Update(levelsRef.Collection(Subfolders.GetPath(l.Key)).Document(l.Key),
                new Dictionary<string,object>
                {
                    { "usageCount", FieldValue.Increment(1) }
                });
        }
        await batch.CommitAsync();
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Load Firestore Data")]
    private async static void LoadFirestoreData()
    {
        string folder = "Assets/Data";
        FirebaseFirestore dataset = FirebaseFirestore.DefaultInstance;
        DocumentReference levelsRef = dataset.Collection(data).Document(levels);

        int count = 0;
        foreach (string sub in Subfolders.Names)
        {
            await ClearCollection(levelsRef.Collection(sub));

            CollectionReference auxRef = levelsRef.Collection(sub);
            foreach (string json in Directory.GetFiles(Path.Combine(folder, sub), "*.json", SearchOption.AllDirectories))
            {
                string levelName = Path.GetFileNameWithoutExtension(json);
                await auxRef.Document(levelName).SetAsync(new Dictionary<string, object>
                {
                    { "usageCount", 0 }
                });
                count++;
            }
        }
        Debug.Log($"Initialized {count} levels in Firestore.");
    }

    private static async Task ClearCollection(CollectionReference collection)
    {
        QuerySnapshot snapshot = await collection.GetSnapshotAsync();
        await Task.WhenAll(snapshot.Documents
            .Select(doc => doc.Reference.DeleteAsync()));
    }
#endif
}

[FirestoreData]
public class FireConfig
{
    [FirestoreProperty] public int humanLevels { get; set; }
    [FirestoreProperty] public int proceduralLevels { get; set; }
    [FirestoreProperty] public int balanceMargin { get; set; }
}

[FirestoreData]
public class FireLevels
{
    public Dictionary<string,FireLevel> HumanLevels = new();
    public Dictionary<string,FireLevel> ProceduralLevels = new();
}

[FirestoreData]
public class FireLevel
{
    [FirestoreProperty] public int usageCount { get; set; }
}

[FirestoreData]
public class FireParticipant
{
    [FirestoreProperty] public int gender { get; set; }
    [FirestoreProperty] public int ageRange { get; set; }
    [FirestoreProperty] public int abilityLevel { get; set; }
    [FirestoreProperty] public Timestamp createdAt { get; set; }

    public FireParticipant() { }
    public FireParticipant(int gender, int ageRange, int abilityLevel)
    {
        this.gender = gender;
        this.ageRange = ageRange;
        this.abilityLevel = abilityLevel;
        createdAt = Timestamp.GetCurrentTimestamp();
    }
}

[Serializable, FirestoreData]
public class FireSession
{
    [FirestoreProperty] private string participantId { get; set; }
    [NonSerialized] public Dictionary<string,FireSessionLevel> levels = new();

    public FireSession() { }
    public FireSession(string participantId)
    {
        this.participantId = participantId;
    }
}

[FirestoreData]
public class FireSessionLevel
{
    [FirestoreProperty] public int satisfaction { get; set; }
    [FirestoreProperty] public int difficulty { get; set; }
    [FirestoreProperty] public bool isperceivedHuman { get; set; }
    [FirestoreProperty] public string comment { get; set; }
    [FirestoreProperty] public int playOrder { get; set; }
    [FirestoreProperty] public double time { get; set; }
    [FirestoreProperty] public int retries { get; set; }

    public FireSessionLevel() { }
    public FireSessionLevel(int satisfaction, int difficulty, bool isperceivedHuman, string comment, int playOrder, double time, int retries)
    {
        this.satisfaction = satisfaction;
        this.difficulty = difficulty;
        this.isperceivedHuman = isperceivedHuman;
        this.comment = comment;
        this.playOrder = playOrder;
        this.time = time;
        this.retries = retries;
    }
}