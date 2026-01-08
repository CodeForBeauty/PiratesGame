

public class Match
{
    static public int MoveTime = 30;
    static public int[] PossibleGridSizes = [7, 9, 11];
    public Server server;
    public Dictionary<IController, int> Clients = [];
    //public Dictionary<PlayerSession, int> Sessions;
    public Grid grid;

    public int CurrentMove = 0;
    public int NextMove = 0;

    private List<int> _leavePlayers = [];
    private int _startPlayers = 0;

    public bool IsPlayingMiniGame = false;
    public Dictionary<int, MiniGameType> PlayingMiniGame = [];
    public List<int> PiratesInMiniGame = [];

    private Timer? _timer = null;

    private List<IController> _losingPlayers = [];

    public bool IsRating = false;

    public HashSet<int> MovingPirates = [];

    public bool IsMoving = false;

    public Match(List<IController> sessions, Server sr)
    {
        server = sr;

        for (int i = 0;  i < sessions.Count; i++)
        {
            sessions[i].SetCurrentMatch(this);
            Clients.Add(sessions[i], i);
        }

        ResponceBase response = new() { Error = NetError.NoError, Result = ResponseResult.MatchStart };
        SendToAllPlayers(response);

        int size = PossibleGridSizes[Clients.Count - 2];
        grid = new Grid(size, size, this);

        _startPlayers = sessions.Count;

        _timeLeft = DateTime.Now.Ticks;
        _timer = new Timer((obj) =>
        {
            _timer?.Dispose();
            _timer = null;
            CalculateNextMove();
        },
                null, 200, Timeout.Infinite);
    }

    public void SendToAllPlayers(object data)
    {
        foreach (IController client in Clients.Keys)
        {
            client.SendData(data);
        }
    }

    public void SendToPlayer(int user, object data)
    {
        IController item = Clients.First(kvp => kvp.Value == user).Key;
        item.SendData(data);
    }

    public void LeaveMatch(PlayerSession session)
    {
        _leavePlayers.Add(Clients[session.controller]);
        while (_leavePlayers.Contains(NextMove))
        {
            NextMove = (NextMove + 1) % _startPlayers;
        }
        if (CurrentMove == Clients[session.controller])
            CurrentMove = NextMove;

        int ind = Clients[session.controller];

        grid.Ships[Clients[session.controller]] = null;
        grid.Pirates[Clients[session.controller]] = null;
        Clients.Remove(session.controller);

        _losingPlayers.Add(session.controller);

        RemovePlayerData data = new() { Result = ResponseResult.RemovePlayer, Error = NetError.NoError, PlayerIndex = ind };
        SendToAllPlayers(data);

        if (Clients.Count == 1)
        {
            _losingPlayers.Add(Clients.Keys.Last());

            CalculateWinner();
            _timer?.Dispose();
        }
    }

    public void MakeMove(IController controller, StepData step)
    {
        if (IsPlayingMiniGame || IsMoving)
            return;
        if ((grid.CurrentMoveTile == -1) && (Clients[controller] != step.Player || step.Player != CurrentMove || (MovingPirates.Count > 0 && !MovingPirates.Contains(step.Pirate))))
            return;

        MovingPirates.Clear();

        if (grid.CurrentOpenTile != -1)
        {
            grid.CurrentOpenTile = -1;
            SentTileData data = new() { Result = ResponseResult.OpenTile, Error = NetError.NoError, Position = step.End, OpenTile = grid.tiles[step.End.x, step.End.y].GetType() };
            SendToPlayer(step.Player, data);
            _timeLeft = DateTime.Now.Ticks;
            _timer = new Timer((obj) =>
            {
                _timer?.Dispose();
                _timer = null;
                CalculateNextMove();
            },
                null, 500, Timeout.Infinite);
            return;
        }

        if (!step.IsCanon && grid.CurrentMoveTile == -1 && (Math.Abs(step.Start.x - step.End.x) > 1 || Math.Abs(step.Start.y - step.End.y) > 1))
        {
            return;
        }

        grid.CurrentMoveTile = -1;

        _timer?.Dispose();
        _timer = null;
        IsMoving = true;

        List<StepData> outputSteps = [];
        grid.MoveLogic(step, ref outputSteps);

        Task.Delay(1000);
        SentStepData stepData = new() { Result = ResponseResult.StepData, Error = NetError.NoError, Data = outputSteps.ToArray() };
        SendToAllPlayers(stepData);
        
        _timeLeft = DateTime.Now.Ticks;
        _timer = new Timer((obj) =>
        {
            _timer?.Dispose();
            _timer = null;
            CalculateNextMove();
        },
                null, 800 * outputSteps.Count, Timeout.Infinite);

        //CalculateNextMove();
    }

    private long _timeLeft = 0;

    public void PauseTimer() {
        if (IsRating) {
            _timeLeft = DateTime.Now.Ticks - _timeLeft;
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }

    public void ResumeTimer() {
        if (IsRating)
            _timer?.Change(_timeLeft, Timeout.Infinite);
    }

    private void CalculateWinner()
    {
        int rating = -(_losingPlayers.Count - 1);
        string[] names = new string[_losingPlayers.Count];
        int[] ratings = new int[_losingPlayers.Count];

        for (int i = 0; i < _losingPlayers.Count; i++)
        {
            PlayerData playerData = _losingPlayers[i].GetData();

            _losingPlayers[i].AddRating(rating);

            names[i] = playerData.Name;
            ratings[i] = rating;

            rating++;
            if (IsRating)
                rating = rating == 0 ? 1 : rating;
            else
                rating = 0;
        }
        MatchEndData endData = new() { Result = ResponseResult.MatchEnd, Error = NetError.NoError, Names = names, Ratings = ratings };
        foreach (IController controller in _losingPlayers)
        {
            controller.SendData(endData);
        }

        server.matches.Remove(this);
    }

    private void CalculateNextMove()
    {
        _timer?.Dispose();
        _timer = null;
        IsMoving = false;
        int alivePlayers = 0;
        foreach (int i in grid.AlivePirates) {
            if (i > 0)
                alivePlayers++;
        }
        if (grid.GoldLeft <= 0 || alivePlayers <= 1)
        {
            for (int i = 0; i < grid.Ships.Length; i++)
            {
                /*if (grid.Ships[i] == null)
                    continue;*/

                int leastGold = 0;
                int leastIndex = 0;
                for (int j = 0; j < grid.Ships.Length; j++)
                {
                    if (grid.Ships[j] == null)
                        continue;
                    if (grid.Ships[j].Gold < leastGold)
                    {
                        leastGold = grid.Ships[j].Gold;
                        leastIndex = j;
                    }
                }

                _losingPlayers.Add(Clients.First(kvp => kvp.Value == leastIndex).Key);
                grid.Ships[leastIndex] = null;
            }

            CalculateWinner();
            return;
        }

        CurrentMove = NextMove;
        NextMove = (NextMove + 1) % _startPlayers;


        while (_leavePlayers.Contains(NextMove) || grid.AlivePirates[NextMove] == 0)
        {
            NextMove = (NextMove + 1) % _startPlayers;
        }

        MoveType moveType = MoveType.Normal;
        if (grid.CurrentOpenTile != -1)
            moveType = MoveType.OpenTile;
        if (grid.CurrentMoveTile != -1)
            moveType = MoveType.MoveTo;
        if (IsPlayingMiniGame)
        {
            NextMoveData dataForPlayers = new() { Result = ResponseResult.MakeStep, Error = NetError.NoError, CurrentPlayer = CurrentMove, Type = MoveType.RockPaperScisors, MoveTime = MoveTime };
            foreach (int i in PlayingMiniGame.Keys)
            {
                Clients.First(x => x.Value == i).Key.SendData(dataForPlayers);
            }
            _timeLeft = DateTime.Now.Ticks;
            _timer = new Timer((obj) =>
            {
                _timer?.Dispose();
                _timer = null;
                IsPlayingMiniGame = false;
                ResetMiniGame();
                CalculateNextMove();
            },
                null, 1200 * MoveTime, Timeout.Infinite);
            return;
        }

        /*grid.CurrentMoveTile = -1;
        grid.CurrentOpenTile = -1;*/

        
        NextMoveData moveData = new() { Result = ResponseResult.MakeStep, Error = NetError.NoError, CurrentPlayer = CurrentMove, Type = moveType, MoveTime = MoveTime, OpenPirates = MovingPirates.ToList() };
        SendToAllPlayers(moveData);


        _timeLeft = DateTime.Now.Ticks;
        _timer = new Timer((obj) =>
        {
            _timer?.Dispose();
            _timer = null;
            CalculateNextMove();
        },
                null, 1200 * MoveTime, Timeout.Infinite);
    }

    public void ResetMiniGame()
    {
        IsPlayingMiniGame = false;
        PlayingMiniGame.Clear();
        PiratesInMiniGame.Clear();
    }

    public void MiniGameMove(IController controller, MiniGameType type)
    {
        if (!IsPlayingMiniGame)
            return;

        int item = Clients.First(kvp => kvp.Key == controller).Value;
        PlayingMiniGame[item] = type;

        foreach (MiniGameType playerChosen in PlayingMiniGame.Values)
        {
            if (playerChosen == MiniGameType.NotChosen)
                return;
        }

        MiniGameType type1 = PlayingMiniGame.Values.First();
        MiniGameType type2 = PlayingMiniGame.Values.Last();

        List<StepData> outputSteps = [];

        if ((type1 == MiniGameType.Rock) && (type2 == MiniGameType.Scisors) ||
            (type1 == MiniGameType.Scisors && type2 == MiniGameType.Paper) ||
            (type1 == MiniGameType.Paper && type2 == MiniGameType.Rock))
        {
            Int2 pos = grid.Pirates[PlayingMiniGame.Keys.First()][PiratesInMiniGame[0]].Position;
            grid.Pirates[PlayingMiniGame.Keys.First()][PiratesInMiniGame[0]].HasGold = grid.Pirates[PlayingMiniGame.Keys.Last()][PiratesInMiniGame[^1]].HasGold;
            outputSteps.Add(new StepData()
            {
                Start = pos,
                End = pos,
                OpenTile = grid.tiles[pos.x, pos.y].GetType(),
                Player = PlayingMiniGame.Keys.First(),
                Pirate = PiratesInMiniGame[0],
                HasGold = grid.Pirates[PlayingMiniGame.Keys.Last()][PiratesInMiniGame[^1]].HasGold
            });

            grid.Pirates[PlayingMiniGame.Keys.Last()][PiratesInMiniGame[^1]].HasGold = false;
            grid.MoveLogic(new StepData()
            {
                Start = grid.Pirates[PlayingMiniGame.Keys.Last()][PiratesInMiniGame[^1]].Position,
                End = grid.Ships[PlayingMiniGame.Keys.Last()].Position,
                OpenTile = TileType.Ship,
                Player = PlayingMiniGame.Keys.Last(),
                Pirate = PiratesInMiniGame[^1],
                AddGold = 0
            }, ref outputSteps);
        }
        else if ((type2 == MiniGameType.Rock) && (type1 == MiniGameType.Scisors) ||
            (type2 == MiniGameType.Scisors && type1 == MiniGameType.Paper) ||
            (type2 == MiniGameType.Paper && type1 == MiniGameType.Rock))
        {
            Int2 pos = grid.Pirates[PlayingMiniGame.Keys.Last()][PiratesInMiniGame[^1]].Position;
            grid.Pirates[PlayingMiniGame.Keys.Last()][PiratesInMiniGame[^1]].HasGold = grid.Pirates[PlayingMiniGame.Keys.First()][PiratesInMiniGame[0]].HasGold;
            outputSteps.Add(new StepData()
            {
                Start = pos,
                End = pos,
                OpenTile = grid.tiles[pos.x, pos.y].GetType(),
                Player = PlayingMiniGame.Keys.Last(),
                Pirate = PiratesInMiniGame[0],
                HasGold = grid.Pirates[PlayingMiniGame.Keys.First()][PiratesInMiniGame[0]].HasGold
            });

            grid.Pirates[PlayingMiniGame.Keys.First()][PiratesInMiniGame[0]].HasGold = false;
            grid.MoveLogic(new StepData()
            {
                Start = grid.Pirates[PlayingMiniGame.Keys.First()][PiratesInMiniGame[0]].Position,
                End = grid.Ships[PlayingMiniGame.Keys.First()].Position,
                OpenTile = TileType.Ship,
                Player = PlayingMiniGame.Keys.First(),
                Pirate = PiratesInMiniGame[0],
                AddGold = 0
            }, ref outputSteps);
        }
        else
        {
            grid.MoveLogic(new StepData()
            {
                Start = grid.Pirates[PlayingMiniGame.Keys.First()][PiratesInMiniGame[0]].Position,
                End = grid.Ships[PlayingMiniGame.Keys.First()].Position,
                OpenTile = TileType.Ship,
                Player = PlayingMiniGame.Keys.First(),
                Pirate = PiratesInMiniGame[0]
            }, ref outputSteps);
            grid.MoveLogic(new StepData()
            {
                Start = grid.Pirates[PlayingMiniGame.Keys.Last()][PiratesInMiniGame[^1]].Position,
                End = grid.Ships[PlayingMiniGame.Keys.Last()].Position,
                OpenTile = TileType.Ship,
                Player = PlayingMiniGame.Keys.Last(),
                Pirate = PiratesInMiniGame[^1]
            }, ref outputSteps);
        }


        MiniGameType[] types = new MiniGameType[grid.Ships.Length];
        types[PlayingMiniGame.Keys.First()] = type1;
        types[PlayingMiniGame.Keys.Last()] = type2;
        MiniGameData gameResult = new() { Result = ResponseResult.MiniGameMove, Error = NetError.NoError, types =  types };
        SendToAllPlayers(gameResult);


        SentStepData stepData = new() { Result = ResponseResult.StepData, Error = NetError.NoError, Data = outputSteps.ToArray() };
        SendToAllPlayers(stepData);

        ResetMiniGame();

        _timer?.Dispose();
        _timeLeft = DateTime.Now.Ticks;
        _timer = new Timer((obj) =>
        {
            _timer?.Dispose();
            _timer = null;
            CalculateNextMove();
        },
                null, 420 * outputSteps.Count, Timeout.Infinite);
    }
}