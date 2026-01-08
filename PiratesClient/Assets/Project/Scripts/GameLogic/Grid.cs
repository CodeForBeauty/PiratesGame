using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Grid : MonoBehaviour
{
    public UnityClient client;

    public TileType[,] Tiles;
    public List<List<Pirate>> Pirates;
    public List<Ship> Ships;

    [SerializeField] private Color _selectColor;
    [SerializeField] private Color _closedColor;

    [SerializeField] private GameObject TilePrefab;
    [SerializeField] private float TileSize = 1f;

    [SerializeField] private GameObject ShipPrefab;
    private GameObject[] ShipsSprite;

    [SerializeField] private GameObject PiratePrefab;
    private List<List<GameObject>> PiratesSprites;

    private SpriteRenderer[,] _sprites;

    [SerializeField] private SpriteData[] _spData;
    private Hashtable _texSource = new();
    private Hashtable _audioSource = new();

    public int SelectedPirate = -1;
    private int _currentPlayer = -1;

    [SerializeField] private ProfileData[] _profiles;
    [SerializeField] private Image[] _selectImages;
    [SerializeField] private Timer _timer;

    [SerializeField] private MatchEndScreen _endScreen;

    private bool _avoidChecks = false;


    public bool PlacingDinamite;
    [SerializeField] private GameObject _dinamitePrefab;
    private Dictionary<Int2, GameObject> _dinamitesPositions = new();
    [SerializeField] private GameObject _boomPrefab;

    [SerializeField] private GameObject _rockPaperScisors;

    [SerializeField] private Color[] _teamsColors;

    public float[] CameraSizes;
    private List<int> _gridSizes = new() { 9, 11, 13 };
    static public float Volume = 1;


    [SerializeField] private GameObject _moveToTileIcon;
    [SerializeField] private GameObject _openTileIcon;
    [SerializeField] private GameObject _dinamiteIcon;

    [SerializeField] private GameObject _canon;
    private bool _fireCanon = false;
    static public Int2[] ShipDirections = new Int2[]
    {
        new Int2() { x =  4, y =  0 },
        new Int2() { x = -4, y =  0 },
        new Int2() { x =  0, y =  4 },
        new Int2() { x =  0, y = -4 },
    };


    private void Awake()
    {
        foreach (SpriteData sp in _spData)
        {
            _texSource.Add(sp.Type, sp.Tex);
            if (sp.Sound != null)
                _audioSource.Add(sp.Type, sp.Sound);
        }
    }

    public void ReceiveData(string json)
    {
        ClearChildren();

        SentMatchStartData data = JsonUtility.FromJson<SentMatchStartData>(json);

        _currentPlayer = data.CurrentPlayer;

        transform.position = new Vector2(data.GridSize.x / 2 * -TileSize, data.GridSize.y / 2 * -TileSize);

        Camera.main.orthographicSize = CameraSizes[_gridSizes.IndexOf(data.GridSize.x)];

        Tiles = new TileType[data.GridSize.x, data.GridSize.y];
        _sprites = new SpriteRenderer[data.GridSize.x, data.GridSize.y];

        for (int i = 0; i < data.GridSize.x; i++)
            for (int j = 0;  j < data.GridSize.y; j++)
            {
                GameObject obj = Instantiate(TilePrefab, GetPosition(i, j), Quaternion.identity, transform);
                TileLogic tileLogic = obj.GetComponent<TileLogic>();
                tileLogic.grid = this;
                tileLogic.position = new Int2() { x = i, y = j };
                _sprites[i, j] = obj.GetComponent<SpriteRenderer>();
                _sprites[i, j].sprite = (Sprite)_texSource[data.Tiles[i * data.GridSize.y + j]];
                Tiles[i, j] = data.Tiles[i * data.GridSize.y + j];
            }

        Pirates = new List<List<Pirate>>();
        ShipsSprite = new GameObject[data.Ships.Length];
        PiratesSprites = new List<List<GameObject>>();
        for (int i = 0; i < data.Ships.Length; i++)
        {
            Pirates.Add(new List<Pirate>());
            PiratesSprites.Add(new List<GameObject>());
            ShipsSprite[i] = Instantiate(ShipPrefab, GetPosition(data.Ships[i].Position.x, data.Ships[i].Position.y), Quaternion.identity, transform);
            ShipsSprite[i].GetComponent<SpriteRenderer>().color = _teamsColors[i];
        }
        foreach (Pirate p in data.Pirates)
        {
            Pirates[p.Team].Add(p);
            GameObject obj = Instantiate(PiratePrefab, GetPosition(p.Position.x, p.Position.y), Quaternion.identity, transform);
            if (p.Team != _currentPlayer)
            {
                obj.GetComponent<Collider2D>().enabled = false;
            }
            else
            {
                obj.GetComponent<Collider2D>().enabled = true;
            }
            PirateLogic pirateLogic = obj.GetComponent<PirateLogic>();
            pirateLogic.Player = p.Team;
            pirateLogic.grid = this;
            pirateLogic.ThisPirate = Pirates[p.Team].Count - 1;
            pirateLogic.Initialize(_teamsColors[p.Team]);
            PiratesSprites[p.Team].Add(obj);
        }
        Ships = new List<Ship>(data.Ships);

        foreach (ProfileData profile in _profiles)
        {
            profile.gameObject.SetActive(false);
        }

        for (int i = 0; i < data.Names.Length; i++)
        {
            _profiles[i].LoadProfile(data.Names[i], $"{data.Countries[i]} - {data.Cities[i]}", data.Ratings[i].ToString(), false);

            InGameProfile prof = _profiles[i].gameObject.GetComponent<InGameProfile>();

            prof.ResetData();
            prof.SetEnableDinamite(_currentPlayer == i);
        }
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public Vector2 GetPosition(int indX, int indY)
    {
        return new()
        {
            x = indX * TileSize + transform.position.x,
            y = indY * TileSize + transform.position.y
        };
    }

    public void SelectPirate(int player, int newPirate)
    {
        if (player != _currentPlayer)
            return;

        if (newPirate == SelectedPirate) {
            if (Tiles[Pirates[_currentPlayer][SelectedPirate].Position.x, Pirates[_currentPlayer][SelectedPirate].Position.y] == TileType.Door) {
                SendMoveCommand(Pirates[_currentPlayer][SelectedPirate].Position);
            }
            return;
        }

        CheckTiles(Pirates[player][newPirate].Position);
        PiratesSprites[player][newPirate].GetComponent<SpriteRenderer>().color = _selectColor;
        if (SelectedPirate != -1)
            PiratesSprites[player][SelectedPirate].GetComponent<SpriteRenderer>().color = Color.white;
        SelectedPirate = newPirate;
    }

    public void SendMoveCommand(Int2 newPos)
    {
        if (PlacingDinamite)
        {
            if (Tiles[newPos.x, newPos.y] == TileType.NotOpen ||
                Tiles[newPos.x, newPos.y] == TileType.Ship ||
                Tiles[newPos.x, newPos.y] == TileType.Water)
            {
                PlacingDinamite = false;

                DisableTiles();
                SelectedPirate = -1;
                DisablePirates(true);

                return;
            }

            StepData step = new() { AddDinamite = true, Start = newPos, End = newPos, Player = _currentPlayer };
            ReceivedData data = new() { PlayerCommand = Command.Move, MoveData = step };

            client.SendMoveCommand(data);

            SelectedPirate = -1;

            GameObject obj = Instantiate(_dinamitePrefab, GetPosition(newPos.x, newPos.y), Quaternion.identity, transform);
            _dinamitesPositions.Add(newPos, obj);

            
        }
        if (!_avoidChecks && SelectedPirate == -1 && !_fireCanon)
            return;

        if ((_fireCanon && !newPos.Equals(new Int2() {
            x = Ships[_currentPlayer].Position.x + ShipDirections[_currentPlayer].x,
            y = Ships[_currentPlayer].Position.y + ShipDirections[_currentPlayer].y
            })) ||
            (!_avoidChecks && SelectedPirate > -1 && ((Math.Abs(Pirates[_currentPlayer][SelectedPirate].Position.x - newPos.x) > 1) ||
            Math.Abs(Pirates[_currentPlayer][SelectedPirate].Position.y - newPos.y) > 1))
            )
        {
            DeselectPirates();
            DisableTiles();
            SelectedPirate = -1;
            return;
        }
        ReceivedData sendData = new() { PlayerCommand = Command.Move, 
            MoveData = new StepData() { Start = SelectedPirate != -1 ? Pirates[_currentPlayer][SelectedPirate].Position : new Int2(),
                                        End = newPos, Player = _currentPlayer, Pirate = SelectedPirate, IsCanon = _fireCanon } };

        client.SendMoveCommand(sendData);
    }

    public void ReceiveMoveData(string json)
    {
        //print(json);
        SentStepData data = JsonUtility.FromJson<SentStepData>(json);

        StartCoroutine(MovePirates(data.Data));
    }

    public void ReceiveMiniGame(string json) {
        MiniGameData data = JsonUtility.FromJson<MiniGameData>(json);

        int index = 0;
        foreach (MiniGameType i in data.types) {
            _profiles[index].gameObject.GetComponent<InGameProfile>().ShowMiniGameResult(((int)i) - 1);
            index++;
        }
    }

    private IEnumerator FireCanon(int player, Int2 pos)
    {
        _canon.SetActive(true);

        _canon.transform.position = ShipsSprite[player].transform.position;

        float startTime = 2;
        float time = startTime;

        

        while (time > 0) 
        {
            // reversed
            _canon.transform.position = Vector3.Lerp(GetPosition(pos.x, pos.y), ShipsSprite[player].transform.position, time / startTime);

            time -= Time.deltaTime;
            yield return null;
        }

        _canon.SetActive(false);
        _fireCanon = false;
    }

    private IEnumerator MovePirates(StepData[] steps)
    {
        foreach (StepData step in steps)
        {
            if (step.AddGold != -1)
            {
                _profiles[step.Player].gameObject.GetComponent<InGameProfile>().AddGold(step.AddGold);
                PiratesSprites[step.Player][step.Pirate].GetComponent<PirateLogic>().GoldSprite.SetActive(false);
            }
            if (step.HasGold)
            {
                PiratesSprites[step.Player][step.Pirate].GetComponent<PirateLogic>().GoldSprite.SetActive(true);
            }

            if (step.AddDinamite)
            {
                _profiles[step.Player].gameObject.GetComponent<InGameProfile>().AddDinamite(1);

                _dinamiteIcon.SetActive(true);
                Invoke(nameof(DisableAllIcons), 5);
            }
            if (step.UseDinamite)
            {
                _profiles[step.Player].gameObject.GetComponent<InGameProfile>().AddDinamite(-1);

                continue;
            }

            if (step.WasBlown)
            {
                Instantiate(_boomPrefab, GetPosition(step.End.x, step.End.y), Quaternion.identity);

                if (_dinamitesPositions.ContainsKey(step.Start))
                    Destroy(_dinamitesPositions[step.Start]);
                _dinamitesPositions.Remove(step.Start);
            }

            if (step.IsCanon) 
            {
                StartCoroutine(FireCanon(step.Player, step.End));

                _profiles[step.Player].gameObject.GetComponent<InGameProfile>().UseCanon();
            }


            if (step.End.x > -1 && step.End.y > -1)
            {
                Tiles[step.End.x, step.End.y] = step.OpenTile;
                _sprites[step.End.x, step.End.y].sprite = (Sprite)_texSource[step.OpenTile];
            }

            if (step.MoveShip) {
                Ships[step.Player].Position = step.End;

                for (int i = 0; i < Pirates[step.Player].Count; i++) {
                    if (Pirates[step.Player][i].Position.x == step.Start.x &&
                        Pirates[step.Player][i].Position.y == step.Start.y) {
                        Pirates[step.Player][i].Position = step.End;
                        StartCoroutine(PiratesSprites[step.Player][i].GetComponent<PirateLogic>().MoveTo(GetPosition(step.Start.x, step.Start.y), GetPosition(step.End.x, step.End.y)));
                    }
                }

                yield return ShipsSprite[step.Player].GetComponent<ShipLogic>().MoveTo(GetPosition(step.Start.x, step.Start.y), GetPosition(step.End.x, step.End.y));
            }
            else if (step.Pirate > -1) {
                Pirates[step.Player][step.Pirate].Position = step.End;
                yield return PiratesSprites[step.Player][step.Pirate].GetComponent<PirateLogic>().MoveTo(GetPosition(step.Start.x, step.Start.y), GetPosition(step.End.x, step.End.y));
            }

            if (_audioSource.ContainsKey(step.OpenTile))
                AudioSource.PlayClipAtPoint((AudioClip)_audioSource[step.OpenTile], Camera.main.transform.position, Volume);
        }
    }

    public void ReceiveNextMove(string json)
    {
        NextMoveData data = JsonUtility.FromJson<NextMoveData>(json);

        if (SelectedPirate != -1)
            CheckTiles(Pirates[_currentPlayer][SelectedPirate].Position);

        if (data.Type == MoveType.RockPaperScisors)
        {
            _rockPaperScisors.SetActive(true);
            DisablePirates(true);
        }
        else if (data.CurrentPlayer == _currentPlayer)
        {
            int i = 0;
            foreach (GameObject pirate in PiratesSprites[data.CurrentPlayer])
            {
                if (data.OpenPirates.Count > 0)
                    pirate.GetComponent<Collider2D>().enabled = data.Type == MoveType.Normal && data.OpenPirates.Contains(i);
                else
                    pirate.GetComponent<Collider2D>().enabled = data.Type == MoveType.Normal;
                i++;
            }
            if (data.Type == MoveType.OpenTile || data.Type == MoveType.MoveTo)
            {
                EnableTiles();
                DisablePirates(true);
                _avoidChecks = true;

                _rockPaperScisors.SetActive(false);
            }


        }
        else
        {
            DeselectPirates();
            DisableTiles();
            DisablePirates(false);
            SelectedPirate = -1;

            _rockPaperScisors.SetActive(false);
            _avoidChecks = false;
        }

        PlacingDinamite = false;

        if (data.Type == MoveType.MoveTo) {
            _moveToTileIcon.SetActive(true);
            Invoke(nameof(DisableAllIcons), 5);
        }
        else if (data.Type == MoveType.OpenTile) {
            _openTileIcon.SetActive(true);
            Invoke(nameof(DisableAllIcons), 5);
        }


        foreach (Image image in _selectImages)
        {
            image.color = new Color(0.65f, 0.65f, 0.65f);
        }

        _selectImages[data.CurrentPlayer].color = new Color(0.9f, 0.9f, 0.9f);

        _timer.CurrentTime = data.MoveTime;
    }

    private void DisableAllIcons() {
        _moveToTileIcon.SetActive(false);
        _openTileIcon.SetActive(false);
        _dinamiteIcon.SetActive(false);
    }

    public void ReceiveRemovePlayer(string json)
    {
        RemovePlayerData data = JsonUtility.FromJson<RemovePlayerData>(json);

        foreach (GameObject pirate in PiratesSprites[data.PlayerIndex])
        {
            Destroy(pirate);
        }

        _profiles[data.PlayerIndex].gameObject.SetActive(false);
        Destroy(ShipsSprite[data.PlayerIndex]);
    }

    public void ReceiveMatchEnd(string json)
    {
        MatchEndData data = JsonUtility.FromJson<MatchEndData>(json);

        _endScreen.LoadProfiles(data.Names, data.Ratings);
    }

    public void DeselectPirates()
    {
        foreach (GameObject obj in PiratesSprites[_currentPlayer])
        {
            obj.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    public void CheckTiles(Int2 pos)
    {
        for (int i = 0; i < _sprites.GetLength(0); i++)
        {
            for (int j = 0; j < _sprites.GetLength(1); j++)
            {
                if (Math.Abs(pos.x - i) <= 1 && Math.Abs(pos.y - j) <= 1)
                {
                    _sprites[i, j].color = _selectColor;
                }
                else
                {
                    _sprites[i, j].color = _closedColor;
                }
            }
        }
    }

    public void DisableTiles()
    {
        for (int i = 0; i < _sprites.GetLength(0); i++)
        {
            for (int j = 0; j < _sprites.GetLength(1); j++)
            {
                _sprites[i, j].color = Color.white;
            }
        }
    }

    public void EnableTiles()
    {
        for (int i = 0; i < _sprites.GetLength(0); i++)
        {
            for (int j = 0; j < _sprites.GetLength(1); j++)
            {
                _sprites[i, j].color = _selectColor;
            }
        }
    }

    public void DisablePirates(bool isDisabled)
    {
        foreach (GameObject obj in PiratesSprites[_currentPlayer])
        {
            obj.GetComponent<Collider2D>().enabled = !isDisabled;
        }
    }

    public void PlaceDinamite()
    {
        if (Ships[_currentPlayer].Dinamites <= 0)
            return;

        EnableTiles();
        DeselectPirates();
        DisablePirates(true);
        PlacingDinamite = true;
    }

    public void StartFireCanon() 
    {
        _fireCanon = true;

        DeselectPirates();
        DisablePirates(true);
        EnableTiles();

        _sprites[Ships[_currentPlayer].Position.x + ShipDirections[_currentPlayer].x, Ships[_currentPlayer].Position.y + ShipDirections[_currentPlayer].y].color = Color.white;
    }

    public void ReceiveOpenTile(string json)
    {
        SentTileData data = JsonUtility.FromJson<SentTileData>(json);

        Tiles[data.Position.x, data.Position.y] = data.OpenTile;
        _sprites[data.Position.x, data.Position.y].sprite = (Sprite)_texSource[data.OpenTile];
    }
}


[Serializable]
public struct SpriteData
{
    public TileType Type;
    public Sprite Tex;
    public AudioClip Sound;
}