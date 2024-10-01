using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomWalkSettings", menuName = "Dungeon Generation Settings/Random Walk")]
public class RandomWalkSettingsSO : ScriptableObject
{
    public int Iterations;
    public int WalkLength;

    [Tooltip("If on, each iteration starts at random point on existing floor - if off, each walk starts from origin point. Turn off for filled-in island shape")]
    public bool StartRandomEachIteration;
}
