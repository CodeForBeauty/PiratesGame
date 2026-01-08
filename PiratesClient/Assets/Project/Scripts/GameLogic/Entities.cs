using System;



[Serializable]
public class Ship
{
    public Int2 Position;
    public int Team;

    public int Dinamites = 1;
}

[Serializable]
public class Pirate
{
    public Int2 Position;
    public int Team;
}