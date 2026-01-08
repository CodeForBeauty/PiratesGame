


public enum Command
{
    Login,
    Register,
    QuitMatch,
    QuitRoom,
    SearchMatch,
    CreateRoom,
    Move,
    AddAI,
    StartMatch,
    MiniGameMove,
    DataRequest,
    StopSearch,
    PlayWithBots,

    Notifications,
    FriendRequest,
    AcceptRequest,
    DeclineRequest,
    Search,
    ChangeData,

    Invite,
    Join,
    Invited,
    FriendsList,
    StartRoom,

    PauseGame,
    ResumeGame
}

[Serializable]
public class ReceivedData
{
    public string Name { get; set; } = "";
    public string Password { get; set; } = "";
    public Command PlayerCommand { get; set; }
    public StepData? MoveData { get; set; }
    public MiniGameType ChosenType { get; set; }
    public int PlayersCount { get; set; }
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
}

public enum NetError 
{
    UserNotFound,
    UserExists,
    AlreadyOnline,
    WrongPassword,
    WrongStep,
    FaultyData,
    NoError
}

public enum ResponseResult
{
    LogedIn,
    Error,
    MatchWait,
    MatchStart,
    MatchData,
    StepData,
    MakeStep,
    SelectTile,
    OpenTile,
    PlayerData,
    RemovePlayer,
    MatchEnd,
    FriendRequest,
    Notifications,
    Search,
    Joined,
    Invited,
    InvitedList,
    FriendsList,
    LeftRoom,
    NameChanged,
    MiniGameMove
}

[Serializable]
public class ResponceBase
{
    public ResponseResult Result { get; set; }
    public NetError Error { get; set; }
}

public class SentLoginData : ResponceBase
{
    public string? Country { set; get; }
    public string? City { set; get; }
}


public class SentPlayerData : ResponceBase
{
    public string Name { get; set; } = "";
    public int Rating { get; set; }
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
    public PublicData[]? Friends { get; set; }
}

public class SentFriendRequest : ResponceBase
{
    public string Name { get; set; } = "";
}

public class SentNotificationns : ResponceBase
{
    public string[]? Names { get; set; }
}

public class SentSearchResult : ResponceBase
{
    public PublicData[]? Results { get; set; }
}