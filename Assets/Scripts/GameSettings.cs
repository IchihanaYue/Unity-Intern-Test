using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : ScriptableObject
{
    public int BoardSizeX = 7;

    public int BoardSizeY = 7;

    public int MatchesMin = 3;

    public int LevelMoves = 16;

    public float LevelTime = 30f;

    public float TimeForHint = 5f;

    public int BottomRowSize = 5;

    public int LayerCount = 4;

    public int TotalTriples = 16;
}
