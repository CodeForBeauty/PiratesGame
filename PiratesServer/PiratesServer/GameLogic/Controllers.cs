

public interface IController
{
    public void SendData(object data);

    public void SetCurrentMatch(Match match);

    public PlayerData? GetData();

    public void AddRating(int add);
}

public class PlayerController : IController
{
    public PlayerSession Sess;

    public PlayerController(PlayerSession sess)
    {
        Sess = sess;
    }

    public void AddRating(int add)
    {
        Sess.Data.Rating += add;
        Sess.Data.Rating = Sess.Data.Rating < 0 ? 0 : Sess.Data.Rating;
        SaveLoadSystem.SaveData(Sess.Data);
    }

    public PlayerData? GetData()
    {
        return Sess.Data;
    }

    public void SendData(object data)
    {
        Sess.SendData(data);
    }

    public void SetCurrentMatch(Match match)
    {
        Sess.currentMatch = match;
    }
}

public class AIControllerEasy : IController
{
    static private string[] AINames = ["AliceBot", "BobBot", "RichardBot", "ChiBot", "IsaacBot"];
    static private Random rng = new();

    private PlayerData _data = new();

    private Match? currentMatch;
    private TileType[,] _tiles;

    private int _currentIndex;

    static private TileType[] HarmfulTiles = [TileType.Water, TileType.Death, TileType.Crab];

    public AIControllerEasy(Room room)
    {

        _data.Name = AINames[rng.Next(AINames.Length)];
        while (room.Names.Contains(_data.Name))
            _data.Name = AINames[rng.Next(AINames.Length)];
        _data.Rating = rng.Next(20, 25);
    }

    public void AddRating(int add)
    {
        // nothing
    }

    public PlayerData? GetData()
    {
        return _data;
    }

    public void SendData(object data)
    {
        ResponceBase bd = (ResponceBase)data;

        if (bd.Result == ResponseResult.MatchData)
        {
            SentMatchStartData sd = (SentMatchStartData)data;
            _tiles = new TileType[sd.GridSize.x, sd.GridSize.y];
            _currentIndex = sd.CurrentPlayer;

            for (int i = 0; i < sd.GridSize.x; i++)
            {
                for (int j = 0; j < sd.GridSize.y; j++)
                {
                    _tiles[i, j] = sd.Tiles[i * sd.GridSize.y + j];
                }
            }

            /*for (int i = 0; i < _tiles.GetLength(0); i++) {
                for (int j = 0; j < _tiles.GetLength(1); j++) {
                    Console.Write(_tiles[i, j]);
                }
                Console.WriteLine();
            }*/
        }
        else if (bd.Result == ResponseResult.StepData)
        {
            SentStepData stepData = (SentStepData)data;
            foreach (StepData sd in stepData.Data)
            {
                if (sd.End.x >= 0 && sd.End.y >= 0)
                    _tiles[sd.End.x, sd.End.y] = sd.OpenTile;
            }
        }
        else if (bd.Result == ResponseResult.MakeStep)
        {
            NextMoveData md = (NextMoveData)data;

            if (md.Type == MoveType.RockPaperScisors)
            {
                ChooseMiniGame();
            }
            else if (md.CurrentPlayer == _currentIndex)
            {
                CalculateMove(md.OpenPirates);
            }
        }
    }

    private void ChooseMiniGame()
    {
        Array values = Enum.GetValues(typeof(MiniGameType));
        MiniGameType randomValue = (MiniGameType)values.GetValue(rng.Next(values.Length));
        currentMatch.MiniGameMove(this, randomValue);
    }

    private bool IsInHarmful(TileType tile)
    {
        foreach (TileType harmful in HarmfulTiles)
        {
            if (tile == harmful)
                return true;
        }
        return false;
    }

    public void SetCurrentMatch(Match match)
    {
        currentMatch = match;
    }

    private void CalculateMove(List<int> pirates)
    {
        if (pirates.Count == 0) {
            for (int i = 0; i < currentMatch.grid.Pirates[_currentIndex].Count; i++)
                pirates.Add(i);
        }

        Dictionary<Int2, int> openMoves = [];
        Dictionary<Int2, float> moveScores = [];

        for (int i = 0; i < _tiles.GetLength(0); i++) {
            for (int j = 0; j < _tiles.GetLength(1); j++) {
                for (int pirate = 0; pirate < pirates.Count; pirate++) {
                    Int2 piratePos = currentMatch.grid.Pirates[_currentIndex][pirate].Position;
                    if ((piratePos.x != i && piratePos.y != j) &&
                        Math.Abs(piratePos.x - i) <= 1 && Math.Abs(piratePos.y - j) <= 1) {
                        /*if (IsInHarmful(_tiles[i, j]))
                            continue;*/
                        Int2 pos = new() { x = i, y = j };
                        openMoves.Add(pos, pirate);
                        moveScores.Add(pos, GetMoveScore(piratePos, pos, currentMatch.grid.Pirates[_currentIndex][pirate].HasGold));
                        break;
                    }
                }
            }
        }


        int currentMovePirate = -1;
        float highestScore = -100;
        Int2 bestMove = new();

        foreach (Int2 move in openMoves.Keys) {
            if (highestScore <= moveScores[move]) {
                highestScore = moveScores[move];
                currentMovePirate = openMoves[move];
                bestMove = move;
            }
        }


        StepData output = new() {
            Player = _currentIndex,
            Pirate = currentMovePirate,
            Start = currentMatch.grid.Pirates[_currentIndex][currentMovePirate].Position,
            End = bestMove
        };

        currentMatch.MakeMove(this, output);
    }

    static private TileType[] arrowTiles = [TileType.ArrowLeft, TileType.ArrowUpLeft, TileType.ArrowUp, 
                            TileType.ArrowUpRight, TileType.ArrowRight, TileType.ArrowDownRight,
                            TileType.ArrowDown, TileType.ArrowDownLeft];

    static private float goldBackGuarantee = 0.95f;

    private float GetMoveScore(Int2 start, Int2 end, bool hasGold) {
        if (_tiles[end.x, end.y] != TileType.NotOpen) {
            if (_tiles[end.x, end.y] == TileType.ArrowDirection) {
                return GetMoveScore(end, ((MoveTile)currentMatch.grid.tiles[end.x, end.y]).Simulate(start, end), hasGold) + GetGoldScore(end);
            }
            if (arrowTiles.Contains(_tiles[end.x, end.y])) {
                return GetMoveScore(end, ((Arrow)currentMatch.grid.tiles[end.x, end.y]).Simulate(end), hasGold) + GetGoldScore(end);
            }
            Int2 shipPos = currentMatch.grid.Ships[_currentIndex].Position;
            if (_tiles[end.x, end.y] == TileType.Door) {
                Int2 otherDoor = ((DoorTile)currentMatch.grid.tiles[end.x, end.y]).OtherDoor;
                return GetGoldScore(otherDoor) + GetGoldBackScore(shipPos, otherDoor);
            }
            if (IsInHarmful(_tiles[end.x, end.y])) {
                return -1;
            }

            float goldBack = 0;
            if (hasGold) {
                goldBack = GetGoldBackScore(shipPos, end);
                return goldBack;
            }

            return GetGoldScore(end) + goldBack;
        }
        return 1 + GetGoldScore(end);
    }

    private float GetGoldBackScore(Int2 shipPos, Int2 end) {
        return (_tiles.GetLength(0) - (Math.Abs(shipPos.x - end.x) + Math.Abs(shipPos.y - end.y))) * goldBackGuarantee;
    }

    static private float goldMultiplier = 0.5f;
    static private TileType[] goldTiles = [TileType.Gold1, TileType.Gold2, TileType.Gold3, TileType.Gold4];
    private float GetGoldScore(Int2 pos) {
        float closestScore = 0;

        for (int i = 0; i < _tiles.GetLength(0); i++) {
            for (int j = 0; j < _tiles.GetLength(1); j++) {
                if (!goldTiles.Contains(_tiles[i, j]))
                    continue;
                float currentScore = Math.Abs(i - pos.x) + Math.Abs(j - pos.y);
                if (closestScore > currentScore) {
                    closestScore = currentScore;
                }
            }
        }

        return (_tiles.GetLength(0) - closestScore) * goldMultiplier;
    }

    /*private void CalculateMove(List<int> pirates) {

        int currentPirate = rng.Next(currentMatch.grid.Pirates[_currentIndex].Count);
        while (currentMatch.grid.Pirates[_currentIndex][currentPirate] == null || currentMatch.grid.Pirates[_currentIndex][currentPirate].IsDead)
            currentPirate = rng.Next(currentMatch.grid.Pirates[_currentIndex].Count);
        Int2 piratePos = currentMatch.grid.Pirates[_currentIndex][currentPirate].Position;


        List<Int2> openMoves = [];
        for (int i = 0; i < _tiles.GetLength(0); i++) {
            for (int j = 0; j < _tiles.GetLength(1); j++) {
                if (Math.Abs(piratePos.x - i) <= 1 && Math.Abs(piratePos.y - j) <= 1) {
                    if (IsInHarmful(_tiles[i, j]))
                        continue;
                    openMoves.Add(new Int2() { x = i, y = j });
                }
            }
        }

        Int2 end = openMoves.Count == 0 ? piratePos : openMoves[rng.Next(openMoves.Count)];

        StepData output = new() {
            Player = _currentIndex,
            Pirate = currentPirate,
            Start = piratePos,
            End = end
        };

        currentMatch.MakeMove(this, output);
    }*/
}