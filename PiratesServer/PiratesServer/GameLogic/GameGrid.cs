

public class Grid {
    public Match match;

    public Ship?[] Ships;
    public List<List<Pirate>?> Pirates;
    public List<int> AlivePirates;
    public int[] Dinamites;
    public int[] Canons;
    public ITile[,] tiles;

    public bool[,] closedTiles;

    static public int StartingPirates = 3;

    static public Dictionary<Type, int> spawnCount = new()
    {
        { typeof(DeathTile), 1 },
        { typeof(AidTile), 2 },
        { typeof(DoorTile), 2 },
        { typeof(CrabTile), 4 },
        { typeof(IslandTile), 1 },
        { typeof(ToShipTile), 2 },
        { typeof(ChestTile), 3 },
        { typeof(GinieTile), 4 },
        { typeof(GoldTile), 25 },
        { typeof(Arrow), 5 },
        { typeof(MoveTile), 3 }
    };

    static public Dictionary<int, float> countMultiplier = new() 
    { 
        { 9, 1.3f },
        { 11, 1.5f },
        { 13, 2.0f },
    };

    public int GoldLeft = 25;

    static public Float2[] ShipPositions =
    [
        new Float2() { x = 0.0f, y = 0.5f },
        new Float2() { x = 1.0f, y = 0.5f },
        new Float2() { x = 0.5f, y = 0.0f },
        new Float2() { x = 0.5f, y = 1.0f }
    ];
    static public Int2[] ShipDirections =
    [
        new Int2() { x =  4, y =  0 },
        new Int2() { x = -4, y =  0 },
        new Int2() { x =  0, y =  4 },
        new Int2() { x =  0, y = -4 },
    ];

    private Random rng;
    private TileType[] VisibleGrid;

    public int CurrentOpenTile = -1;
    public int CurrentMoveTile = -1;

    public HashSet<Int2> DinamitePositions = [];

    

    public Grid(int sizeX, int sizeY, Match currentMatch)
    {
        rng = new Random();
        match = currentMatch;

        Ships = new Ship[currentMatch.Clients.Count];
        Dinamites = new int[currentMatch.Clients.Count];
        Canons = new int[currentMatch.Clients.Count];
        Pirates = new List<List<Pirate>?>();
        AlivePirates = new List<int>();

        tiles = new ITile[sizeX + 2, sizeY + 2];
        for (int  i = 0;  i < sizeX + 2;  i++)
        {
            for (int j = 0; j < sizeY + 2; j++)
            {
                tiles[i, j] = new EmptyTile();
            }
        }
        closedTiles = new bool[sizeX + 2, sizeY + 2];
        VisibleGrid = new TileType[(sizeX + 2) * (sizeY + 2)];

        for (int i = 0; i < Ships.Length; i++)
        {
            Dinamites[i] = 1; // Starting dinamites
            Canons[i] = 1; // Starting dinamites
            Int2 pos = new()
            {
                x = (int)MathF.Ceiling(ShipPositions[i].x * (sizeX + 1)),
                y = (int)MathF.Ceiling(ShipPositions[i].y * (sizeY + 1))
            };
            Ships[i] = new Ship() { Position = pos, Team = i };

            Pirates.Add(new List<Pirate>());
            AlivePirates.Add(StartingPirates);
            for (int j = 0; j < StartingPirates; j++)
            {
                Pirates[i].Add(new Pirate() { Position = pos, Team = i });
            }

            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    int posX = pos.x + x;
                    int posY = pos.y + y;
                    if ((posX >= closedTiles.GetLength(0) || posX < 0) || (posY >= closedTiles.GetLength(1) || posY < 0))
                        continue;

                    closedTiles[posX, posY] = true;
                }
            }
        }

        Generate();
    }

    private void Generate()
    {
        int posX;
        int posY;

        DoorTile? lastDoor = null;
        Int2 lastPos = new();
        foreach (Type type in spawnCount.Keys)
        {
            int last;
            if (type == typeof(DoorTile)) 
            {
                last = spawnCount[type];
            }
            else 
            {
                last = (int)(spawnCount[type] * countMultiplier[closedTiles.GetLength(0)]);
            }
            for (int i = 0; i < last; i++)
            {
                while (true)
                {
                    posX = rng.Next(1, closedTiles.GetLength(0) - 1);
                    posY = rng.Next(1, closedTiles.GetLength(1) - 1);
                    if (!closedTiles[posX, posY])
                        break;
                }

                closedTiles[posX, posY] = true;
                object? obj = Activator.CreateInstance(type);
                if (obj != null)
                {
                    tiles[posX, posY] = (ITile)obj;
                    i += tiles[posX, posY].GetCost() - 1;
                    if (tiles[posX, posY].GetType() == TileType.Door)
                    {
                        if (lastDoor == null)
                        {
                            lastDoor = (DoorTile)tiles[posX, posY];
                            lastPos = new Int2() { x = posX, y = posY };
                        }
                        else
                        {
                            lastDoor.OtherDoor = new Int2 { x = posX,y = posY };
                            ((DoorTile)tiles[posX, posY]).OtherDoor = lastPos;
                        }
                    }
                }
                else
                    Console.WriteLine("Error with spawnCount");
            }
        }

        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                if (i == 0 || i == tiles.GetLength(0) - 1 || j == 0 || j == tiles.GetLength(1) - 1) {
                    tiles[i, j] = new WaterTile();
                    VisibleGrid[i * tiles.GetLength(1) + j] = TileType.Water;
                }
                else
                    VisibleGrid[i * tiles.GetLength(1) + j] = TileType.NotOpen;//tiles[i, j].GetType();
            }
        }

        int sizeX = tiles.GetLength(0) - 2;
        int sizeY = tiles.GetLength(1) - 2;

        for (int i = 0; i < Ships.Length; i++)
        {
            Int2 pos = Ships[i].Position;
            tiles[pos.x, pos.y] = new ShipTile();
        }


        List<Pirate> piratesList = new();
        for (int i = 0; i < Pirates.Count; i++)
        {
            for (int j = 0; j < Pirates[i].Count; j++)
            {
                piratesList.Add(Pirates[i][j]);
            }
        }

        string[] names = new string[match.Clients.Count];
        string[] countries = new string[match.Clients.Count];
        string[] cities = new string[match.Clients.Count];
        int[] ratings = new int[match.Clients.Count];

        for (int i = 0; i < match.Clients.Count; i++)
        {
            PlayerData? clientData = match.Clients.First(kvp => kvp.Value == i).Key.GetData();
            if (clientData == null)
                continue;

            names[i] = clientData.Name;
            countries[i] = clientData.Country;
            cities[i] = clientData.City;
            ratings[i] = clientData.Rating;
        }

        Task.Delay(500);
        for (int i = 0; i < match.Clients.Count; i++)
        {
            SentMatchStartData data = new()
            {
                Result = ResponseResult.MatchData,
                Error = NetError.NoError,
                Ships = Ships,
                Pirates = piratesList,
                Tiles = VisibleGrid,
                GridSize = new Int2() { x = tiles.GetLength(0), y = tiles.GetLength(1) },
                CurrentPlayer = i,
                Names = names,
                Ratings = ratings,
                Countries = countries,
                Cities = cities,
            };
            match.SendToPlayer(i, data);
        }
    }


    public void MoveLogic(StepData step, ref List<StepData> data)
    {
        if (step.AddDinamite)
        {
            if (Dinamites[step.Player] <= 0)
                return;
            DinamitePositions.Add(step.End);
            Dinamites[step.Player]--;
            step.Start = new Int2();
            step.End = new Int2();
            step.UseDinamite = true;
            step.AddDinamite = false;
            data.Add(step);
            return;
        }
        else if (step.IsCanon)
        {
            if (!step.End.Equals(new Int2() { x = Ships[step.Player].Position.x + ShipDirections[step.Player].x, 
                                             y = Ships[step.Player].Position.y + ShipDirections[step.Player].y })
                || Canons[step.Player] <= 0)
            {
                Console.WriteLine("not fired");
                return;
            }
            Console.WriteLine("fire");
            Canons[step.Player]--;
            data.Add(step);
            for (int i = 0; i < Pirates.Count; i++) {
                if (Pirates[i] == null) {
                    continue;
                }
                for (int j = 0; j < Pirates[i].Count; j++) 
                {
                    if (Pirates[i][j].Position.Equals(step.End)) 
                    {
                        if (Pirates[i][j].HasGold) 
                        {
                            if (tiles[step.End.x, step.End.y].GetType() == TileType.Empty) {
                                tiles[step.End.x, step.End.y] = new GoldTile();
                                ((GoldTile)tiles[step.End.x, step.End.y]).Cost = 1;

                            }
                            else
                            {
                                ((GoldTile)tiles[step.End.x, step.End.y]).Cost++;
                            }
                            data.Add(new StepData() {
                                Start = step.End,
                                End = step.End,
                                OpenTile = tiles[step.End.x, step.End.y].GetType(),
                                Player = i,
                                Pirate = j,
                                AddGold = 0
                            });
                            Pirates[i][j].HasGold = false;
                        }

                        MoveLogic(new StepData() {
                            Start = step.End,
                            End = Ships[i].Position,
                            OpenTile = TileType.Ship,
                            Player = i,
                            Pirate = j
                        }, ref data);
                    }
                }
            }
            return;
        }

        ITile tile = tiles[step.End.x, step.End.y];

        step.OpenTile = tiles[step.End.x, step.End.y].GetType();
        Pirates[step.Player][step.Pirate].Position = step.End;
        data.Add(step);

        tile.GoTo(this, ref data);

        if (data.Count == 0)
            return;

        if (DinamitePositions.Contains(data[^1].End))
        {
            DinamitePositions.Remove(data[^1].End);
            MoveLogic(new StepData()
            {
                Start = data[^1].End,
                End = Ships[data[^1].Player].Position,
                OpenTile = TileType.Ship,
                Player = data[^1].Player,
                Pirate = data[^1].Pirate,
                WasBlown = true
            }, ref data);
            return;
        }

        if (tile.GetType() == TileType.Island)
            return;

        bool isInSameTile = false;
        //Int2 ind = new Int2();
        int otherPlayer = 0;
        int otherPirate = 0;
        for (int i = 0; i < Pirates.Count; i++)
        {
            if (i == step.Player || Pirates[i] == null)
                continue;
            for (int j = 0; j < Pirates[i].Count; j++)
            {
                if ((Pirates[i][j].Position.x == step.End.x) && (Pirates[i][j].Position.y == step.End.y))
                {
                    otherPlayer = i;
                    otherPirate = j;
                    //ind = new Int2() { x = i, y = j };
                    isInSameTile = true; 
                    break;
                }
            }
            if (isInSameTile)
                break;
        }

        if (isInSameTile)
        {
            match.NextMove = match.CurrentMove;
            match.IsPlayingMiniGame = true;
            match.PlayingMiniGame.Add(step.Player, MiniGameType.NotChosen);
            match.PlayingMiniGame.Add(otherPlayer, MiniGameType.NotChosen);

            match.PiratesInMiniGame.Add(step.Pirate);
            match.PiratesInMiniGame.Add(otherPirate);
        }
    }
}