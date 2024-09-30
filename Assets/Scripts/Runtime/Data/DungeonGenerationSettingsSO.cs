using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomWalkSettings", menuName = "Dungeon Generation Settings/Random Walk")]
public class RandomWalkSettingsSO : ScriptableObject
{

    public int Iterations;
    public int WalkLength;
    public bool StartRandomEachIteration;
}
