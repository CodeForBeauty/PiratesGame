

public interface ITile
{
    public void GoTo(Grid grid, ref List<StepData> steps);

    public int GetCost();

    public TileType GetType();
}


/*Смерть - 1 клетка(умирает персонаж, золото пропадает)
Аптечка - 2 клетки(восстановливается персонаж, если их меньше 3 - ёх)
Двери - 2 клетки(портал, перебрасывающий игрока между клетками)
Крабы - 4 клетки(персонаж наступающий на краба, возвращается на прежнюю клетку)
Остров - 1 клетка(нельзя атаковать персонажа, находящегося на этой клетке)
Корабль - 2 клетки(быстрое возвращение на корабль)
Сундук - тайник - 3 клетки
Джин - 4 клетки (дополнительный ход)
Золото - 25 шт (максимум 4 золота на 1 клетку)*/

public class EmptyTile : ITile
{
    public void GoTo(Grid grid, ref List<StepData> steps) { }

    public int GetCost()
    {
        return 1;
    }

    TileType ITile.GetType()
    {
        return TileType.Empty;
    }
}

public class DeathTile : ITile
{
    public void GoTo(Grid grid, ref List<StepData> steps) 
    {
        steps.Add(new()
        {
            Start = new Int2() { x = -50, y = -50 },
            End = new Int2() { x = -50, y = -50 },
            OpenTile = TileType.Empty,
            Player = steps[^1].Player,
            Pirate = steps[^1].Pirate,
            AddGold = 0
        });
        grid.Pirates[steps[^1].Player][steps[^1].Pirate].Position = new Int2() { x = -50, y = -50 };
        grid.Pirates[steps[^1].Player][steps[^1].Pirate].IsDead = true;
        grid.Pirates[steps[^1].Player][steps[^1].Pirate].HasGold = true;
        grid.AlivePirates[steps[^1].Player]--;

        if (grid.Pirates[steps[^1].Player][steps[^1].Pirate].HasGold)
            grid.GoldLeft--;
    }

    public int GetCost()
    {
        return 1;
    }

    TileType ITile.GetType()
    {
        return TileType.Death;
    }
}

public class AidTile : ITile
{
    private bool isUsed = false;
    public void GoTo(Grid grid, ref List<StepData> steps) 
    {
        if (isUsed)
            return;
        int deadPirate = -1;
        for (int i = 0; i < grid.Pirates[steps[^1].Player].Count; i++)
        {
            if (grid.Pirates[steps[^1].Player][i].IsDead)
            {
                deadPirate = i;
                break;
            }
        }
        if (deadPirate == -1)
            return;
        isUsed = true;
        steps.Add(new()
        {
            Start = steps[^1].End,
            End = steps[^1].End,
            OpenTile = TileType.Empty,
            Player = steps[^1].Player,
            Pirate = deadPirate
        });
        grid.Pirates[steps[^1].Player][deadPirate].Position = steps[^1].End;
        grid.Pirates[steps[^1].Player][deadPirate].IsDead = false;
        grid.AlivePirates[steps[^1].Player]++;
    }

    public int GetCost()
    {
        return 1;
    }

    TileType ITile.GetType()
    {
        return isUsed ? TileType.Empty : TileType.Aid;
    }
}

public class DoorTile : ITile
{
    public Int2 OtherDoor;

    public void GoTo(Grid grid, ref List<StepData> steps) 
    {
        /*Int2 secondDoor = new();
        bool isFound = false;
        for (int i = 0; i < grid.tiles.GetLength(0); i++)
        {
            for (int j = 0; j < grid.tiles.GetLength(1); j++)
            {
                if (steps[^1].End.x != i && steps[^1].End.y != j && grid.tiles[i, j].GetType() == TileType.Door)
                {
                    secondDoor = new Int2() { x = i, y = j };
                    isFound = true;
                    break;
                }
            }
            if (isFound)
                break;
        }*/

        steps.Add(new()
        {
            Start = steps[^1].End,
            End = OtherDoor,
            OpenTile = TileType.Door,
            Player = steps[^1].Player,
            Pirate = steps[^1].Pirate
        });
        grid.Pirates[steps[^1].Player][steps[^1].Pirate].Position = OtherDoor;
    }

    public int GetCost()
    {
        return 1;
    }

    TileType ITile.GetType()
    {
        return TileType.Door;
    }
}

public class CrabTile : ITile
{
    public void GoTo(Grid grid, ref List<StepData> steps) 
    {
        grid.MoveLogic(new()
        {
            Start = steps[^1].End,
            End = steps[^1].Start,
            OpenTile = grid.tiles[steps[^1].Start.x, steps[^1].Start.y].GetType(),
            Player = steps[^1].Player,
            Pirate = steps[^1].Pirate
        }, ref steps);
        /*steps.Add(new()
        {
            Start = steps[^1].End,
            End = steps[^1].Start,
            OpenTile = grid.tiles[steps[^1].Start.x, steps[^1].Start.y].GetType(),
            Player = steps[^1].Player,
            Pirate = steps[^1].Pirate
        });
        grid.Pirates[steps[^1].Player][steps[^1].Pirate].Position = steps[^1].End;*/
    }

    public int GetCost()
    {
        return 1;
    }

    TileType ITile.GetType()
    {
        return TileType.Crab;
    }
}

public class IslandTile : ITile
{
    public void GoTo(Grid grid, ref List<StepData> steps) { }

    public int GetCost()
    {
        return 1;
    }

    TileType ITile.GetType()
    {
        return TileType.Island;
    }
}

public class ToShipTile : ITile
{
    public void GoTo(Grid grid, ref List<StepData> steps) 
    {
        grid.MoveLogic(new()
        {
            Start = steps[^1].End,
            End = grid.Ships[steps[^1].Player].Position,
            OpenTile = TileType.Ship,
            Player = steps[^1].Player,
            Pirate = steps[^1].Pirate
        }, ref steps);
        /*steps.Add(new()
        {
            Start = steps[^1].End,
            End = grid.Ships[steps[^1].Player].Position,
            OpenTile = TileType.Water,
            Player = steps[^1].Player,
            Pirate = steps[^1].Pirate
        });
        grid.Pirates[steps[^1].Player][steps[^1].Pirate].Position = steps[^1].End;*/
    }

    public int GetCost()
    {
        return 1;
    }

    TileType ITile.GetType()
    {
        return TileType.ToShip;
    }
}

public class ChestTile : ITile
{
    private static readonly Random rng = new();
    private int _chestType;
    private bool isOpen = false;

    public ChestTile()
    {
        _chestType = rng.Next(3);
    }

    public void GoTo(Grid grid, ref List<StepData> steps) 
    {
        if (isOpen)
            return;
        isOpen = true;

        steps.Add(new()
        {
            Start = steps[^1].End,
            End = steps[^1].End,
            OpenTile = TileType.ChestOpen,
            Player = steps[^1].Player,
            Pirate = steps[^1].Pirate
        });

        switch (_chestType)
        {
            case (0):
                grid.match.NextMove = grid.match.CurrentMove;
                grid.CurrentOpenTile = steps[^1].Pirate;
                break;
            case (1):
                grid.match.NextMove = grid.match.CurrentMove;
                grid.CurrentMoveTile = steps[^1].Pirate;
                break;
            case (2):
                grid.Dinamites[steps[^1].Player]++;
                steps[^1].AddDinamite = true;
                break;
        }
    }

    public int GetCost()
    {
        return 1;
    }

    TileType ITile.GetType()
    {
        return isOpen ? TileType.ChestOpen : TileType.Chest;
    }
}

public class GinieTile : ITile
{
    public void GoTo(Grid grid, ref List<StepData> steps) 
    {
        grid.match.MovingPirates.Add(steps[^1].Pirate);
        grid.match.NextMove = grid.match.CurrentMove;
    }

    public int GetCost()
    {
        return 1;
    }

    TileType ITile.GetType()
    {
        return TileType.Ginie;
    }
}

public class GoldTile : ITile
{
    private static readonly Random rng = new();
    private static readonly TileType[] types = [TileType.Empty, TileType.Gold1, TileType.Gold2, TileType.Gold3, TileType.Gold4];
    public int Cost = 4;

    public GoldTile()
    {
        Cost = rng.Next(2, 5);
    }

    public void GoTo(Grid grid, ref List<StepData> steps) 
    {
        if (Cost <= 0 || grid.Pirates[steps[^1].Player][steps[^1].Pirate].HasGold)
            return;
        grid.Pirates[steps[^1].Player][steps[^1].Pirate].HasGold = true;
        Cost--;
        steps.Add(new()
        {
            Start = steps[^1].End,
            End = steps[^1].End,
            OpenTile = types[Cost],
            Player = steps[^1].Player,
            Pirate = steps[^1].Pirate,
            HasGold = true,
        });
    }

    public int GetCost()
    {
        return Cost;
    }

    TileType ITile.GetType()
    {
        return types[Cost];
    }
}

public class ShipTile : ITile
{

    public void GoTo(Grid grid, ref List<StepData> steps)
    {
        foreach (Ship? ship in grid.Ships)
        {
            if (ship == null) 
                continue;
            if (ship.Position.x == steps[^1].End.x && ship.Position.y == steps[^1].End.y)
            {
                if (steps[^1].Player != ship.Team)
                {
                    steps.Remove(steps[^1]);
                    return;
                }
            }
        }
        if (!grid.Pirates[steps[^1].Player][steps[^1].Pirate].HasGold)
            return;
        steps.Add(new()
        {
            Start = steps[^1].End,
            End = steps[^1].End,
            OpenTile = TileType.Ship,
            Player = steps[^1].Player,
            Pirate = steps[^1].Pirate,
            AddGold = 1
        });
        grid.Pirates[steps[^1].Player][steps[^1].Pirate].HasGold = false;
        grid.Ships[steps[^1].Player].Gold++;
        grid.GoldLeft--;
    }

    public int GetCost()
    {
        return 1;
    }

    TileType ITile.GetType()
    {
        return TileType.Ship;
    }
}

public class WaterTile : ITile
{
    public void GoTo(Grid grid, ref List<StepData> steps) 
    {
        StepData lastStep = steps[^1];
        int player = steps[^1].Player;
        Int2 shipPos = grid.Ships[player].Position;
        if (shipPos.x == lastStep.Start.x && 
            shipPos.y == lastStep.Start.y) {
            shipPos = lastStep.End;
            // Swap water with ship tiles
            grid.Ships[player].Position = lastStep.End;
            (grid.tiles[lastStep.End.x, lastStep.End.y], grid.tiles[lastStep.Start.x, lastStep.Start.y]) = (grid.tiles[lastStep.Start.x, lastStep.Start.y], grid.tiles[lastStep.End.x, lastStep.End.y]);

            for (int i = 0; i < grid.Pirates[player].Count; i++) {
                Int2 piratePos = grid.Pirates[player][i].Position;
                if (piratePos.x == shipPos.x && piratePos.y == shipPos.y) {
                    grid.Pirates[player][i].Position = lastStep.End;
                }
            }

            steps[^1].MoveShip = true;
            return;
        }

        steps.Add(new()
        {
            Start = new Int2() { x = -50, y = -50 },
            End = new Int2() { x = -50, y = -50 },
            OpenTile = TileType.Water,
            Player = player,
            Pirate = lastStep.Pirate,
            AddGold = 0
        });
        grid.Pirates[player][lastStep.Pirate].Position = new Int2() { x = -50, y = -50 };
        grid.Pirates[player][lastStep.Pirate].IsDead = true;
        grid.AlivePirates[player]--;

        if (grid.Pirates[player][lastStep.Pirate].HasGold)
            grid.GoldLeft--;
    }

    public int GetCost()
    {
        return 1;
    }

    TileType ITile.GetType()
    {
        return TileType.Water;
    }
}

public class Arrow : ITile
{
    private Int2 direction;
    private static readonly Int2[] directions = [
        new Int2() { x =  1, y =  0 },
        new Int2() { x =  1, y =  1 },
        new Int2() { x =  0, y =  1 },
        new Int2() { x = -1, y =  1 },
        new Int2() { x = -1, y =  0 },
        new Int2() { x = -1, y = -1 },
        new Int2() { x =  0, y = -1 },
        new Int2() { x =  1, y = -1 },
    ];
    private static readonly Dictionary<Int2, TileType> types = new() {
        {new Int2() { x =  1, y =  0 }, TileType.ArrowRight},
        {new Int2() { x =  1, y =  1 }, TileType.ArrowUpRight},
        {new Int2() { x =  0, y =  1 }, TileType.ArrowUp},
        {new Int2() { x = -1, y =  1 }, TileType.ArrowUpLeft},
        {new Int2() { x = -1, y =  0 }, TileType.ArrowLeft},
        {new Int2() { x = -1, y = -1 }, TileType.ArrowDownLeft},
        {new Int2() { x =  0, y = -1 }, TileType.ArrowDown},
        { new Int2() { x = 1, y = -1 }, TileType.ArrowDownRight}
    };
    private static readonly Random rng = new();

    public Arrow()
    {
        direction = directions[rng.Next(0, directions.Length)];
    }

    public void GoTo(Grid grid, ref List<StepData> steps) 
    {
        if (steps.Count > 1 && (steps[^1].End.x == steps[^2].Start.x && steps[^1].End.y == steps[^2].Start.y))
        {
            steps.Add(new()
            {
                Start = new Int2() { x = -50, y = -50 },
                End = new Int2() { x = -50, y = -50 },
                OpenTile = TileType.Empty,
                Player = steps[^1].Player,
                Pirate = steps[^1].Pirate
            });
            grid.Pirates[steps[^1].Player][steps[^1].Pirate].Position = new Int2() { x = -50, y = -50 };
            grid.Pirates[steps[^1].Player][steps[^1].Pirate].IsDead = true;
            grid.AlivePirates[steps[^1].Player]--;
            return;
        }
        grid.MoveLogic(new()
        {
            Start = steps[^1].End,
            End = new Int2() { x = steps[^1].End.x + direction.x, y = steps[^1].End.y + direction.y },
            OpenTile = grid.tiles[steps[^1].End.x + direction.x, steps[^1].End.y + direction.y].GetType(),
            Player = steps[^1].Player,
            Pirate = steps[^1].Pirate
        }, ref steps);
    }

    public int GetCost()
    {
        return 1;
    }

    TileType ITile.GetType()
    {
        return types[direction];
    }

    public Int2 Simulate(Int2 end) {
        return new Int2() { x = end.x + direction.x, y = end.y + direction.y };
    }
}

public class MoveTile : ITile
{
    public void GoTo(Grid grid, ref List<StepData> steps) 
    {
        Int2 direction = new() { x = steps[^1].End.x - steps[^1].Start.x, y = steps[^1].End.y - steps[^1].Start.y };
        grid.MoveLogic(new()
        {
            Start = steps[^1].End,
            End = new Int2() { x = steps[^1].End.x + direction.x, y = steps[^1].End.y + direction.y },
            OpenTile = grid.tiles[steps[^1].End.x + direction.x, steps[^1].End.y + direction.y].GetType(),
            Player = steps[^1].Player,
            Pirate = steps[^1].Pirate
        }, ref steps);
    }

    public int GetCost()
    {
        return 1;
    }

    TileType ITile.GetType()
    {
        return TileType.ArrowDirection;
    }

    public Int2 Simulate(Int2 start, Int2 end) {
        Int2 direction = new() { x = end.x - start.x, y = end.y - start.y };
        return new Int2() { x = end.x + direction.x, y = end.y + direction.y };
    }
}