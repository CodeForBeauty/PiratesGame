using NetCoreServer;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

public class PlayerSession {
    protected Server ServerCh;
    public PlayerData? Data;

    public Room? currentRoom;
    public Match? currentMatch;

    public PlayerController controller;

    public HashSet<string> friendRequests = [];

    public Dictionary<string, Room?> Invited = [];

    public Guid SessId;

    public Action<byte[]> SendOverNet;

    public PlayerSession(Server server, Guid Id) {
        SessId = Id;
        controller = new PlayerController(this);
        ServerCh = server;
    }

    public void OnDisconnect() {
        ServerCh.RemoveFromQueues(SessId);
        if (Data != null)
            ServerCh.playersOnline.Remove(Data.Name);
        currentRoom?.LeaveRoom(this);
        currentMatch?.LeaveMatch(this);
        Console.WriteLine($"[INFO] Player with ID: {SessId} disconnected!");
    }

    public void OnReceive(byte[] buffer, long offset, long size) {
        ReceivedData? receivedData;
        string json;
        try {
            json = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            receivedData = JsonSerializer.Deserialize<ReceivedData>(json.Trim());
            if (receivedData == null) {
                ResponceBase response = new() { Error = NetError.FaultyData, Result = ResponseResult.Error };
                SendData(response);
            }
        }
        catch (Exception ex) {
            ResponceBase response = new() { Error = NetError.FaultyData, Result = ResponseResult.Error };
            SendData(response);
            Console.WriteLine($"[ERROR] {ex.Source}");
            return;
        }

        if (receivedData == null)
            return;

        try {
            ProcessInput(receivedData);
        }
        catch (Exception ex) {
            Console.WriteLine($"[ERROR] {ex.Message}");
            return;
        }
    }

    private void ProcessInput(ReceivedData receivedData) {
        switch (receivedData.PlayerCommand) {
            // Login
            case (Command.Login):
                Login(receivedData.Name, receivedData.Password);
                break;
            case (Command.Register):
                Register(receivedData.Name, receivedData.Password);
                break;
            case (Command.ChangeData):
                if (!string.IsNullOrEmpty(receivedData.Country) && !string.IsNullOrEmpty(receivedData.City)) {
                    Data.Country = receivedData.Country;
                    Data.City = receivedData.City;
                    SaveLoadSystem.SaveData(Data);
                }

                ResponceBase res = new() { Result = ResponseResult.NameChanged };

                if (Data.Name != receivedData.Name && !string.IsNullOrEmpty(receivedData.Name))
                    res.Error = SaveLoadSystem.ChangeName(Data.Name, receivedData.Name) ? NetError.NoError : NetError.UserExists;
                SendData(res);
                break;
            // Request data
            case (Command.DataRequest):
                RequestData(receivedData.Name);
                break;
            case (Command.Notifications):
                RequestNotifications();
                break;
            case (Command.Search):
                RequestSearch(receivedData.Name, receivedData.Country, receivedData.City);
                break;
            case (Command.FriendsList):
                PublicData[] friends = SaveLoadSystem.GetFriends(receivedData.Name);
                for (int i = 0; i < friends.Length; i++) {
                    friends[i].IsOnline = ServerCh.playersOnline.ContainsKey(friends[i].Name);
                }
                SendData(new SentSearchResult() { Result = ResponseResult.FriendsList, Error = NetError.NoError, Results = friends });
                break;
            // Friend requests
            case (Command.FriendRequest):
                ServerCh.FriendRequest(Data.Name, receivedData.Name);
                break;
            case (Command.AcceptRequest):
                AcceptFriend(receivedData.Name);
                break;
            case (Command.DeclineRequest):
                ServerCh.friendRequests[Data.Name].Remove(receivedData.Name);
                break;
            // Search match
            case (Command.SearchMatch):
                ServerCh.AddToQueue(SessId, Data, receivedData.PlayersCount);
                break;
            case (Command.PlayWithBots):
                ServerCh.CreateRoom([new WaitData() { Name = Data.Name, Id = SessId, Rating = Data.Rating }], receivedData.PlayersCount);
                currentRoom.IsRating = false;
                for (int i = 0; i < receivedData.PlayersCount; i++)
                    currentRoom?.AddAI(new AIControllerEasy(currentRoom));
                break;
            case (Command.StopSearch):
                ServerCh.RemoveFromQueues(SessId);
                break;
            case (Command.AddAI):
                if (currentRoom != null && this == currentRoom.Owner) {
                    currentRoom.AddAI(new AIControllerEasy(currentRoom));
                }
                break;
            // Matches control
            case (Command.QuitMatch):
                currentMatch?.LeaveMatch(this);
                currentMatch = null;
                break;
            case (Command.QuitRoom):
                currentRoom?.LeaveRoom(this);
                currentRoom = null;
                break;
            case (Command.CreateRoom):
                ServerCh.CreateRoom([new WaitData() { Name = Data.Name, Id = SessId, Rating = Data.Rating }], 4);
                currentRoom.Owner = this;
                currentRoom.IsRating = false;
                break;
            case (Command.Invite):
                InvitePlayer(receivedData.Name);
                break;
            case (Command.Join):
                JoinRoom(receivedData.Name);
                break;
            case (Command.Invited):
                InvitedList();
                break;
            case (Command.StartRoom):
                currentRoom?.StartGame();
                break;
            // In game moves
            case (Command.Move):
                currentMatch?.MakeMove(controller, receivedData.MoveData);
                break;
            case (Command.MiniGameMove):
                currentMatch?.MiniGameMove(controller, receivedData.ChosenType);
                break;
            case (Command.PauseGame):
                currentMatch?.PauseTimer();
                break;
            case (Command.ResumeGame):
                currentMatch?.ResumeTimer();
                break;
            default:
                ResponceBase response = new() { Error = NetError.FaultyData, Result = ResponseResult.Error };
                SendData(response);
                break;
        }
    }

    

    protected bool Login(string username, string password) {
        SentLoginData response = new() { Error = NetError.NoError, Result = ResponseResult.LogedIn };
        bool output = true;


        if (!SaveLoadSystem.LoadData(username, out Data)) {
            response.Error = NetError.UserNotFound;
            output = false;
        }
        else if (ServerCh.playersOnline.ContainsKey(username)) {
            response.Error = NetError.AlreadyOnline;
            output = false;
        }
        else if (Data.Password != password) {
            response.Error = NetError.WrongPassword;
            output = false;
        }
        else {
            response.Country = Data.Country;
            response.City = Data.City;
            AuthPlayer();
        }

        if (Data != null) {
            for (int i = 0; i < Data.Friends.Length; i++) {
                Data.Friends[i].IsOnline = ServerCh.playersOnline.ContainsKey(Data.Friends[i].Name);
            }
        }


        SendData(response);
        return output;
    }

    protected bool Register(string username, string password) {
        ResponceBase response = new() { Error = NetError.NoError, Result = ResponseResult.LogedIn };
        bool output = true;

        Data = new PlayerData() { Name = username, Password = password };

        if (!SaveLoadSystem.CreateData(Data)) {
            response.Error = NetError.UserExists;
            response.Result = ResponseResult.LogedIn;
            output = false;
        }

        SendData(response);
        AuthPlayer();
        return output;
    }

    protected void AuthPlayer() {
        ServerCh.playersOnline.Add(Data.Name, this);
    }


    public void SendData(object data) {
        MemoryStream stream = new();
        JsonSerializer.Serialize(stream, data);
        SendOverNet(stream.ToArray());
        stream.Dispose();
    }

    public void RequestData(string login) {
        bool success = SaveLoadSystem.LoadData(login, out PlayerData? foundData);


        SentPlayerData data;
        if (!success) {
            data = new() { Result = ResponseResult.PlayerData, Error = NetError.UserNotFound };
        }
        else {
            for (int i = 0; i < foundData.Friends.Length; i++) {
                foundData.Friends[i].IsOnline = ServerCh.playersOnline.ContainsKey(foundData.Friends[i].Name);
            }
            data = new() { Result = ResponseResult.PlayerData, Error = NetError.NoError, Name = foundData.Name, Rating = foundData.Rating, Friends = foundData.Friends };
        }

        SendData(data);
    }

    public void AcceptFriend(string name) {
        if (!ServerCh.friendRequests[Data.Name].Contains(name))
            return;

        ServerCh.friendRequests[Data.Name].Remove(name);
        if (ServerCh.friendRequests[Data.Name].Count == 0)
            ServerCh.friendRequests.Remove(Data.Name);

        PublicData[] tmp = new PublicData[Data.Friends.Length + 1];
        Data.Friends.CopyTo(tmp, 0);
        tmp[^1] = SaveLoadSystem.GetPlayerData(name);
        Data.Friends = tmp;

        SaveLoadSystem.AddFriends(Data.Name, name);
    }

    public void NotifyFriendRequest(string name) {
        SendData(new SentFriendRequest() { Result = ResponseResult.FriendRequest, Error = NetError.NoError, Name = name });
    }

    public bool HasFriend(string name) {
        foreach (PublicData friend in Data.Friends) {
            if (friend.Name == name)
                return true;
        }
        return false;
    }

    public void RequestNotifications() {
        if (Data == null)
            return;
        if (!ServerCh.friendRequests.ContainsKey(Data.Name))
            SendData(new SentNotificationns() { Result = ResponseResult.Notifications, Error = NetError.NoError, Names = [] });
        else
            SendData(new SentNotificationns() { Result = ResponseResult.Notifications, Error = NetError.NoError, Names = ServerCh.friendRequests[Data.Name].ToArray() });
    }

    public void RequestSearch(string query, string country, string city) {
        SentSearchResult data = new() { Result = ResponseResult.Search, Error = NetError.NoError, Results = SaveLoadSystem.SearchPlayers(query, country, city) };

        SendData(data);
    }

    public void InvitePlayer(string name) {
        if (currentRoom == null || !ServerCh.playersOnline.ContainsKey(name))
            return;

        ((PlayerSession?)ServerCh.playersOnline[name])?.Invite(currentRoom);
    }

    public void Invite(Room room) {
        Invited.Add(room.Owner.Data.Name, room);
        SentFriendRequest data = new() { Result = ResponseResult.Invited, Error = NetError.NoError, Name = room.Owner.Data.Name };
        SendData(data);
    }

    public void JoinRoom(string owner) {
        if (!Invited.ContainsKey(owner) || Invited[owner] == null)
            return;

        Invited[owner].AddPlayer(this);
        Invited.Clear();
    }

    public void InvitedList() {
        SentNotificationns data = new() { Result = ResponseResult.InvitedList, Error = NetError.NoError, Names = Invited.Keys.ToArray() };
        SendData(data);
    }
}

public class PlayerSessTcp : TcpSession {
    public PlayerSession Session;

    public PlayerSessTcp(TcpServer tcp, Server server) : base(tcp) {
        Session = new PlayerSession(server, Id);

        Session.SendOverNet += (byte[] data) => { SendAsync(data); };
    }

    protected override void OnConnected() {
        Console.WriteLine($"[INFO] TCP Player connected with ID: {Id}!");
    }

    protected override void OnDisconnected() {
        Session.OnDisconnect();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size) {
        base.OnReceived(buffer, offset, size);
        Session.OnReceive(buffer, offset, size);
    }

    protected override void OnError(SocketError error) {
        Console.WriteLine($"[ERROR] {error}");
    }
}

public class PlayerSessWs : WssSession {
    public PlayerSession Session;

    public PlayerSessWs(WssServer ws, Server server) : base(ws) {
        Session = new PlayerSession(server, Id);

        Session.SendOverNet += (byte[] data) => { SendTextAsync(data); };
    }

    protected override void OnConnected() {
        base.OnConnected();
    }

    public override void OnWsConnected(HttpRequest request) {
        Console.WriteLine($"[INFO] WebSocket Player connected with ID: {Session.SessId}!");
    }

    protected override void OnDisconnected() {
        Session.OnDisconnect();
        base.OnDisconnected();
    }

    public override void OnWsReceived(byte[] buffer, long offset, long size) {
        Session.OnReceive(buffer, offset, size);
    }

    protected override void OnError(SocketError error) {
        Console.WriteLine($"[ERROR] {error}");
        base.OnError(error);
    }
}