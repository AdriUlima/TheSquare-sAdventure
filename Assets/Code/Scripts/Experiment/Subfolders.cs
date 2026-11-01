using System;
using System.Collections.Generic;
using UnityEngine;

public static class Subfolders
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        All = null;
    }

    public const string Human = "HumanLevels", Procedural = "ProceduralLevels";
    public static readonly string[] Names = { Human, Procedural };
    public static SubfolderData[] All { get; private set; }
    
    public static void Initialize(FireConfig fireConfig, FireLevels fireLevels)
    {
        All = new SubfolderData[Names.Length];
        for (int i = 0; i < Names.Length; i++)
        {
            All[i] = new(Names[i])
            {
                Amount = i switch
                {
                    0 => fireConfig.humanLevels,
                    1 => fireConfig.proceduralLevels,
                    _ => throw new ArgumentException(i.ToString())
                },
                Levels = i switch
                {
                    0 => fireLevels.HumanLevels,
                    1 => fireLevels.ProceduralLevels,
                    _ => throw new ArgumentException(i.ToString())
                }
            };
        }
    }
    public static SubfolderData GetSubfolderData(int index) => All[index];

    public static FireLevels InsertDict(FireLevels fireLevels, Dictionary<string,FireLevel> aux, int i)
    {
        switch (i)
        {
            case 0:
                fireLevels.HumanLevels = aux;
                break;
            case 1:
                fireLevels.ProceduralLevels = aux;
                break;
        }
        return fireLevels;
    }

    public static string GetPath(string key) => key.StartsWith('H')? Human : Procedural;
}

public class SubfolderData
{
    public string Name;
    public int Amount;
    public Dictionary<string,FireLevel> Levels;

    public SubfolderData(string name)
    {
        Name = name;
    }
}