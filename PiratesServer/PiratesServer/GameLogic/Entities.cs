

public class Ship
{
    public Int2 Position { get; set; }
    public int Team { get; set; }
    public int Gold;
}

public class Pirate
{
    public Int2 Position { get; set; }
    public int Team { get; set; }
    public bool IsDead;
    public bool HasGold;
}