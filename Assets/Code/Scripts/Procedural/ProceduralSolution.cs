using System;
using NaughtyAttributes;
using UnityEngine;
using static ProceduralManager;
using Random = System.Random;

[CreateAssetMenu(fileName = "ProceduralSolution", menuName = "Scriptable Objects/Procedural/Solution")]
public class ProceduralSolution : ScriptableObject
{
    [Header("Solution Settings")]
    [SerializeField] private MinMaxSetting moves = new("Moves", 5, 2, 50);
    public Movement[] solution;

    void OnValidate()
    {
        moves.Validate();
    }

    [Button]
    public void PickMoves()
    {
        Random r = new();
        int m = moves.GetValueSetting();
        Movement[] solution = new Movement[m], d = (Movement[])Enum.GetValues(typeof(Movement));
        solution[0] = d[r.Next(0, d.Length)];

        for (int i = 1; i < m; i++) solution[i] = d[(Array.IndexOf(d, solution[i - 1]) / 2 * 2 + r.Next(2, 4)) % d.Length];
        this.solution = solution;
    }
}