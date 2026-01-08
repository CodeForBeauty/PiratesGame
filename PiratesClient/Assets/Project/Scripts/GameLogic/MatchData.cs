using JetBrains.Annotations;
using System;
using System.Collections.Generic;


[Serializable]
public struct Int2
{
    public int x;
    public int y;
}

[Serializable]
public class StepData
{
    public Int2 Start;
    public Int2 End;
    public int Pirate;
    public int Player;
    public TileType OpenTile;
    public int AddGold = -1;
    public bool HasGold;
    public bool AddDinamite;
    public bool UseDinamite;
    public bool WasBlown;
    public bool MoveShip;
    public bool IsCanon;
}

[Serializable]
public class SentStepData : ResponceBase
{
    public StepData[] Data;
}

public class SentTileData : ResponceBase
{
    public Int2 Position;
    public TileType OpenTile;
}

public enum TileType
{
    NotOpen,
    Empty,
    Water,
    Death,
    Aid,
    Door,
    Crab,
    Island,
    ToShip,
    Chest,
    Ginie,
    Gold1,
    Gold2,
    Gold3,
    Gold4,

    ArrowRight,
    ArrowUpRight,
    ArrowUp,
    ArrowUpLeft,
    ArrowLeft,
    ArrowDownLeft,
    ArrowDown,
    ArrowDownRight,
    ArrowDirection,

    Ship,
    ChestOpen
}

public class SentMatchStartData : ResponceBase
{
    public string[] Names;
    public int[] Ratings;
    public string[] Countries;
    public string[] Cities;
    public Ship[] Ships;
    public List<Pirate> Pirates;
    public Int2 GridSize;
    public TileType[] Tiles;
    public int CurrentPlayer;
}

public enum MoveType
{
    Normal,
    OpenTile,
    MoveTo,
    RockPaperScisors
}

[Serializable]
public class NextMoveData : ResponceBase
{
    public int CurrentPlayer;
    public MoveType Type;
    public float MoveTime;
    public List<int> OpenPirates;
}

public enum MiniGameType
{
    NotChosen,
    Rock,
    Paper,
    Scisors
}

[Serializable]
public class RemovePlayerData : ResponceBase
{
    public int PlayerIndex;
}

public class MatchEndData : ResponceBase
{
    public string[] Names;
    public int[] Ratings;
}

public class MiniGameData : ResponceBase {
    public MiniGameType[] types;
}