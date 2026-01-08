


public class PlayerData
{
    public string Name { get; set; } = "";
    public string Password { get; set; } = "";
    public int Rating { get; set; }

    public string Country { get; set; } = "";
    public string City { get; set; } = "";
    public PublicData[]? Friends { get; set; }
}

[Serializable]
public class PublicData
{
    public string Name { get; set; } = "";
    public int Rating { get; set; }
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
    public bool IsOnline { get; set; }
}