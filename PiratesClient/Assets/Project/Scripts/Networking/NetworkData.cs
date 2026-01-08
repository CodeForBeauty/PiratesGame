using System;


[Serializable]
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
    public string? Name;
    public string? Password;
    public Command PlayerCommand;
    public StepData MoveData;
    public MiniGameType ChosenType;
    public int PlayersCount;
    public string Country;
    public string City;
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
    public ResponseResult Result;
    public NetError Error = NetError.NoError;
}

public class SentLoginData : ResponceBase
{
    public string Country;
    public string City;
}

[Serializable]
public class PublicData
{
    public string Name;
    public int Rating;
    public string Country;
    public string City;
    public bool IsOnline;
}

[Serializable]
public class SentPlayerData : ResponceBase
{
    public string Name;
    public int Rating;
    public string Country;
    public string City;
    public PublicData[] Friends;
}

[Serializable]
public class SentFriendRequest : ResponceBase
{
    public string Name = "";
}
[Serializable]
public class SentNotificationns : ResponceBase
{
    public string[] Names;
}

[Serializable]
public class SentSearchResult : ResponceBase
{
    public PublicData[] Results;
}