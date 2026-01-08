

public class Room
{
    public Server server;
    public List<PlayerSession> Clients = [];
    public List<IController> Controllers = [];
    public HashSet<string> Names = [];
    public int MaxPlayers;

    public bool AutoStart = true;

    public PlayerSession? Owner;

    public bool IsRating = true;
    

    public Room(Server sr, int maxPlayers)
    {
        MaxPlayers = maxPlayers;
        server = sr;
    }

    public void AddPlayer(PlayerSession session)
    {
        Clients.Add(session);
        Controllers.Add(session.controller);
        Names.Add(session.Data.Name);
        List<PublicData> joined = [];
        foreach (IController controller in Controllers)
        {
            PlayerData dat = controller.GetData();
            joined.Add(new() { Name = dat.Name, Rating = dat.Rating, City = dat.City, Country = dat.Country, IsOnline = true });
            if (session.controller != controller)
                controller.SendData(new SentPlayerData() { Result = ResponseResult.Joined, Name = session.Data.Name, Country = session.Data.Country, City = session.Data.City, Rating = session.Data.Rating });
        }

        SentSearchResult response = new() { Error = NetError.NoError, Result = ResponseResult.MatchWait, Results = joined.ToArray() };
        session.SendData(response);

        session.currentRoom = this;
        if (AutoStart && MaxPlayers == Controllers.Count)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (Controllers.Count <= 1)
            return;

        foreach (PlayerSession session in Clients)
        {
            session.currentRoom = null;
        }

        server.CreateMatch(this);
    }

    public void LeaveRoom(PlayerSession session)
    {
        Clients.Remove(session);
        Controllers.Remove(session.controller);
        if (Clients.Count == 0)
        {
            server.rooms.Remove(this);
            return;
        }

        foreach (IController controller in Controllers)
        {
            controller.SendData(new SentFriendRequest() { Result = ResponseResult.LeftRoom, Name = session.Data.Name });
        }
    }

    public void AddAI(IController controller)
    {
        Controllers.Add(controller);
        PlayerData data = controller.GetData();
        Names.Add(data.Name);
        foreach (IController cont in Controllers)
        {
            cont.SendData(new SentPlayerData() { Result = ResponseResult.Joined, Name = data.Name, Country = data.Country, City = data.City, Rating = data.Rating });
        }

        if (AutoStart && MaxPlayers == Controllers.Count)
        {
            StartGame();
        }
    }
}