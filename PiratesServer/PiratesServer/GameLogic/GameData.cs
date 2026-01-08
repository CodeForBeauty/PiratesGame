


[Serializable]
public struct Int2
{
    public int x { get; set; }
    public int y { get; set; }
}

[Serializable]
public struct Float2
{
    public float x { get; set; }
    public float y { get; set; }
}

[Serializable]
public class StepData 
{
    public Int2 Start { get; set; }
    public Int2 End { get; set; }
    public int Pirate { get; set; }
    public int Player { get; set; }
    public TileType OpenTile { get; set; }
    public int AddGold { get; set; } = -1;
    public bool HasGold { get; set; }
    public bool AddDinamite { get; set; }
    public bool UseDinamite { get; set; }
    public bool WasBlown { get; set; }
    public bool MoveShip { get; set; }
    public bool IsCanon { get; set; }
}

[Serializable]
public class SentStepData : ResponceBase
{
    public StepData[]? Data {  get; set; }
}

public class SentTileData : ResponceBase
{
    public Int2 Position { get; set; }
    public TileType OpenTile { get; set; }
}

[Serializable]
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
    public string[]? Names { get; set; }
    public int[]? Ratings { get; set; }
    public string[]? Countries { get; set; }
    public string[]? Cities { get; set; }
    public Ship?[]? Ships { get; set; }
    public List<Pirate>? Pirates { get; set; }
    public Int2 GridSize { get; set; }
    public TileType[]? Tiles { get; set; }
    public int CurrentPlayer { get; set; }
}

public enum MoveType
{
    Normal,
    OpenTile,
    MoveTo,
    RockPaperScisors
}

public class NextMoveData : ResponceBase
{
    public int CurrentPlayer { get; set; }
    public MoveType Type { get; set; }
    public float MoveTime { get; set; }
    public List<int>? OpenPirates { get; set; }
}

public enum MiniGameType
{
    NotChosen,
    Rock,
    Paper,
    Scisors
}

public class RemovePlayerData : ResponceBase
{
    public int PlayerIndex {  get; set; }
}

public class MatchEndData : ResponceBase
{
    public string[]? Names { get; set; }
    public int[]? Ratings { get; set; }
}

public class MiniGameData : ResponceBase {
    public MiniGameType[]? types { get; set; }
}