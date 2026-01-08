using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _playersPos;
    [SerializeField] private Vector3 _playersOffset;

    [Header("Friends")]
    [SerializeField] private GameObject _friendPrefab;
    [SerializeField] private Transform _friendsPos;
    [SerializeField] private Vector3 _friendsOffset;

    public Dictionary<string, GameObject> Players = new();

    public bool IsStartedMatch { get; set; }



    public void ReceiveRoomCreated(string json)
    {
        Players.Clear();
        SentSearchResult data = JsonUtility.FromJson<SentSearchResult>(json);

        for (int i = _playersPos.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_playersPos.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < data.Results.Length; i++)
        {
            GameObject obj = Instantiate(_playerPrefab, _playersPos.position + (_playersOffset * i), Quaternion.identity, _playersPos);

            obj.GetComponent<ProfileData>().LoadProfile(data.Results[i].Name, $"{data.Results[i].Country} - {data.Results[i].City}", data.Results[i].Rating.ToString(), true);

            Players.Add(data.Results[i].Name, obj);
        }

        if (IsStartedMatch)
            gameObject.SetActive(false);
    }

    public void ReceiveJoinedRoom(string json)
    {
        SentPlayerData data = JsonUtility.FromJson<SentPlayerData>(json);
        //if (Players.ContainsKey(data.Name))
        //    return;

        GameObject obj = Instantiate(_playerPrefab, _playersPos.position + (_playersOffset * Players.Count), Quaternion.identity, _playersPos);
        obj.GetComponent<ProfileData>().LoadProfile(data.Name, $"{data.Country} - {data.City}", data.Rating.ToString(), true);
        Players.Add(data.Name, obj);
    }

    public void ReceiveLeftRoom(string json)
    {
        SentFriendRequest data = JsonUtility.FromJson<SentFriendRequest>(json);

        Destroy(Players[data.Name]);
        Players.Remove(data.Name);
    }

    public void ReceiveFriendsList(string json)
    {
        SentSearchResult data = JsonUtility.FromJson<SentSearchResult>(json);

        for (int i = _friendsPos.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_friendsPos.transform.GetChild(i).gameObject);
        }

        int onlineCount = 0;
        for (int i = 0; i < data.Results.Length; i++)
        {
            if (!data.Results[i].IsOnline)
                continue;
            GameObject obj = Instantiate(_friendPrefab, _friendsPos.position + (_friendsOffset * onlineCount), Quaternion.identity, _friendsPos);

            obj.GetComponent<ProfileData>().LoadProfile(data.Results[i].Name, $"{data.Results[i].Country} - {data.Results[i].City}", data.Results[i].Rating.ToString(), true);

            onlineCount++;
        }
    }
}
