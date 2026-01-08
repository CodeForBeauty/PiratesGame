using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UnityClient : MonoBehaviour
{
    [Header("Login")]
    [SerializeField] private GameObject _loginForm;
    [SerializeField] private GameObject _mainMenu;

    [SerializeField] private TMP_InputField _loginField;
    [SerializeField] private TMP_InputField _passwordField;

    [SerializeField] private GameObject _userNotFound;
    [SerializeField] private GameObject _userExists;
    [SerializeField] private GameObject _incorrectPassword;
    [SerializeField] private GameObject _alreadyOnline;

    [Header("Network")]
    [SerializeField] private string _address = "127.0.0.1";
    [SerializeField] private int _port = 8080;
    [SerializeField] private int _portWs = 8080;

    private Client _client;

    [SerializeField] private CommandFunction[] _commands = new CommandFunction[0];
    public Hashtable Commands = new();

    public Dictionary<ResponceBase, UnityEvent<string>> actions = new();
    public Dictionary<ResponceBase, string> passedJson = new();

    public UnityEvent<string> OnConnected = new();

    public string CurrentName;
    public string CurrentCountry;
    public string CurrentCity;

    [Header("Notifications")]
    [SerializeField] private GameObject _notificationPrefab;
    [SerializeField] private Transform _notificationPosition;

    [SerializeField] private TextMeshProUGUI _invite;

    [SerializeField] private AudioClip _notificationFX;

    [SerializeField] private UnityEvent _onLoginError;

    [SerializeField] private GameObject _nameChangeError;

    [SerializeField] private TextAsset _certificate;

    public ChatClient TestClient;

    void Awake()
    {
        foreach (CommandFunction cmd in _commands)
        {
            Commands.Add(cmd.Result, cmd.ResultAction);
        }

#if UNITY_WEBGL
        _client = new Client(_address, _portWs);
#else
        _client = new Client(_address, _port);
#endif
        _client.client = this;
        _client.Conn();

        //TestClient = new(new NetCoreServer.SslContext(SslProtocols.Tls12, new X509Certificate2(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "client.pfx")), "qwerty"), (sender, certificate, chain, sslPolicyErrors) => true), _address, _port);
        //TestClient = new(new NetCoreServer.SslContext(SslProtocols.Tls12, new X509Certificate2(_certificate.bytes, "qwerty"), (sender, certificate, chain, sslPolicyErrors) => true), _address, _portWs);
        //TestClient = new(new NetCoreServer.SslContext(SslProtocols.Tls12, (sender, certificate, chain, sslPolicyErrors) => true), _address, _portWs);
        //TestClient = new(new NetCoreServer.SslContext(SslProtocols.None, (sender, certificate, chain, sslPolicyErrors) => true), Dns.GetHostAddresses("12bitgame.ru").FirstOrDefault(), _portWs);
        //TestClient.Connect();
    }

    private void Update()
    {
        if (actions.Count > 0)
        {
            /*int removed = 0;
            for (int i = 0; i < actions.Count; i++)
            {
                try
                {
                    ResponceBase response = actions.Keys.ElementAt(i - removed);
                    actions[response].Invoke(passedJson[response]);
                    actions.Remove(response);
                    passedJson.Remove(response);
                    removed++;
                }
                catch (Exception e)
                {
                    print(e);
                }
            }*/
            for (int i = actions.Count - 1; i >= 0; i--) {
                try {
                    ResponceBase response = actions.Keys.ElementAt(i);
                    actions[response].Invoke(passedJson[response]);
                    actions.Remove(response);
                    passedJson.Remove(response);
                }
                catch (Exception e) {
                    print(e);
                }
            }
            /*try
            {
                *//*for (int i = actions.Count - 1; i >= 0; i--)
                {
                    ResponceBase response = actions.Keys.ElementAt(i);
                    print(response.Result);
                    actions[response].Invoke(passedJson[response]);
                    actions.Remove(response);
                    passedJson.Remove(response);
                }*/
            /*foreach (ResponceBase i in actions.Keys)
            {
                actions[i].Invoke(passedJson[i]);
                actions.Remove(i);
                passedJson.Remove(i);
            }*//*
            ResponceBase response = actions.Keys.First();
            actions[response].Invoke(passedJson[response]);
            actions.Remove(response);
            passedJson.Remove(response);
            *//*actions.Clear();
            passedJson.Clear();*//*
        }
        catch (Exception e)
        {
            print(e);
        }*/
        }
        _client.DispatchMessages();
    }

    private void OnDestroy()
    {
        _client.DisconnectAndStop();
    }

    public void OnLoginUpdate(string json)
    {
        _incorrectPassword.SetActive(false);
        _userExists.SetActive(false);
        _userNotFound.SetActive(false);
        _alreadyOnline.SetActive(false);

        SentLoginData response = JsonUtility.FromJson<SentLoginData>(json);
        CurrentCountry = response.Country;
        CurrentCity = response.City;
        if (response.Error != NetError.NoError)
        {
            switch (response.Error)
            {
                case (NetError.UserExists):
                    _onLoginError.Invoke();
                    _userExists.SetActive(true);
                    break;
                case (NetError.UserNotFound):
                    _onLoginError.Invoke();
                    _userNotFound.SetActive(true);
                    break;
                case (NetError.WrongPassword):
                    _onLoginError.Invoke();
                    _incorrectPassword.SetActive(true);
                    break;
                case (NetError.AlreadyOnline):
                    _alreadyOnline.SetActive(true);
                    break;
                default:
                    break;
            }
            return;
        }

        _loginForm.SetActive(false);
        _mainMenu.SetActive(true);
    }

    public void OnNameChanged(string json) {
        ResponceBase res = JsonUtility.FromJson<ResponceBase>(json);

        if (res.Error == NetError.UserExists) {
            _nameChangeError.SetActive(true);
        }
    }

    public void SendPause() {
        ReceivedData data = new() { PlayerCommand = Command.PauseGame };

        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void SendResume() {
        ReceivedData data = new() { PlayerCommand = Command.ResumeGame };

        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void SendLogin(string type)
    {
        ReceivedData data = new() { PlayerCommand = Enum.Parse<Command>(type), Name = _loginField.text, Password = _passwordField.text };
        CurrentName = _loginField.text;
        
        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void SendFindMatch(int playerCount)
    {
        ReceivedData data = new() { PlayerCommand = Command.SearchMatch, PlayersCount = playerCount };

        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void SendPlayWithAI(int count)
    {
        ReceivedData data = new() { PlayerCommand = Command.PlayWithBots, PlayersCount = count };

        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void SendMoveCommand(object data)
    {
        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void Reconnect()
    {
        //_client.ConnectAsync();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SendDataRequest()
    {
        ReceivedData data = new() { PlayerCommand = Command.DataRequest, Name = CurrentName };

        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void SendDataRequest(TextMeshProUGUI name)
    {
        ReceivedData data = new() { PlayerCommand = Command.DataRequest, Name = name.text };

        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void StopSearch()
    {
        ReceivedData data = new() { PlayerCommand = Command.StopSearch };

        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void LeaveMatch()
    {
        ReceivedData data = new() { PlayerCommand = Command.QuitMatch };

        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void SendMiniGameData(string type)
    {
        ReceivedData data = new() { PlayerCommand = Command.MiniGameMove, ChosenType = (MiniGameType)Enum.Parse(typeof(MiniGameType), type)};

        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void SendFriendRequest(TextMeshProUGUI name)
    {
        ReceivedData data = new() { PlayerCommand = Command.FriendRequest, Name = name.text };

        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void SendAcceptFriend(TextMeshProUGUI name)
    {
        ReceivedData data = new() { PlayerCommand = Command.AcceptRequest, Name = name.text };

        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void SendDeclineFriend(TextMeshProUGUI name)
    {
        ReceivedData data = new() { PlayerCommand = Command.DeclineRequest, Name = name.text };

        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void SendNotificationsRequest()
    {
        ReceivedData data = new() { PlayerCommand = Command.Notifications };

        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void SendSearchRequest(object data)
    {
        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void ShowNotification(string json)
    {
        SentFriendRequest newRequest = JsonUtility.FromJson<SentFriendRequest>(json);

        GameObject obj = Instantiate(_notificationPrefab, _notificationPosition.position, Quaternion.identity, _notificationPosition);
        obj.AddComponent<TimedEffect>()._lifetime = 5;
        obj.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = newRequest.Name;

        AudioSource.PlayClipAtPoint(_notificationFX, Camera.main.transform.position, Grid.Volume);
    }

    public void RequestFriends()
    {
        ReceivedData data = new() { PlayerCommand = Command.FriendsList, Name = CurrentName };
        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void AddAI()
    {
        ReceivedData data = new() { PlayerCommand = Command.AddAI };
        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void InvitePlayer(TextMeshProUGUI name)
    {
        ReceivedData data = new() { PlayerCommand = Command.Invite, Name = name.text };
        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void RequestInvited()
    {
        ReceivedData data = new() { PlayerCommand = Command.Invited, Name = CurrentName };
        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void SendStartRoom()
    {
        ReceivedData data = new() { PlayerCommand = Command.StartRoom };
        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void ReceiveInvite(string json)
    {
        SentFriendRequest request = JsonUtility.FromJson<SentFriendRequest>(json);

        _invite.gameObject.SetActive(true);
        _invite.text = request.Name;
        Invoke(nameof(HideInvite), 5);

        AudioSource.PlayClipAtPoint(_notificationFX, Camera.main.transform.position, Grid.Volume);
    }

    public void HideInvite()
    {
        _invite.gameObject.SetActive(false);
    }

    public void SendJoinRoom(TextMeshProUGUI owner)
    {
        ReceivedData data = new() { PlayerCommand = Command.Join, Name = owner.text };
        _client.SendDataAsync(JsonUtility.ToJson(data));
    }

    public void SendCreateRoom()
    {
        ReceivedData data = new() { PlayerCommand = Command.CreateRoom, Name = CurrentName };
        _client.SendDataAsync(JsonUtility.ToJson(data));
    }
}

[Serializable]
public class CommandFunction
{
    public ResponseResult Result;
    public UnityEvent<string> ResultAction;
}