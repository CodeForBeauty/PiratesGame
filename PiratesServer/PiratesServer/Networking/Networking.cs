using NetCoreServer;
using System.Net.Sockets;
using System.Net;
using System.Collections;


public class Server
{
    public Hashtable playersOnline = [];
    public MatchMaker[] matchers;

    public HashSet<Room> rooms = [];
    public HashSet<Match> matches = [];

    public Dictionary<string, HashSet<string>> friendRequests = [];

    public Server()
    {
        int matchersCount = 3;
        matchers = new MatchMaker[matchersCount];
        for (int i = 0; i < matchersCount; i++)
        {
            matchers[i] = new MatchMaker(this);
            matchers[i].playerCount = i + 2;
        }
    }

    public void AddToQueue(Guid sessionID, PlayerData playerData, int matchCount)
    {
        WaitData data = new() { Name = playerData.Name, Id = sessionID, Rating = playerData.Rating };
        matchers[matchCount - 2].AddToQueue(data);
    }

    public void RemoveFromQueues(Guid sessionID)
    {
        foreach (MatchMaker matcher in matchers)
        {
            matcher.RemoveFromQueue(sessionID);
        }
    }

    public void CreateRoom(WaitData[] players, int maxPlayers)
    {
        Room room = new(this, maxPlayers);
        foreach (WaitData player in players)
        {
            room.AddPlayer((PlayerSession)playersOnline[player.Name]);
        }
        rooms.Add(room);
    }

    public void CreateMatch(Room inRoom)
    {
        Match match = new(inRoom.Controllers, this);
        match.IsRating = inRoom.IsRating;

        matches.Add(match);

        rooms.Remove(inRoom);
    }

    public void FriendRequest(string requesting, string player)
    {
        if (friendRequests.ContainsKey(player))
            friendRequests[player].Add(requesting);
        else
            friendRequests.Add(player, []);

        if (playersOnline.ContainsKey(player))
        {
            ((PlayerSession)playersOnline[player]).NotifyFriendRequest(requesting);
        }
    }
}
