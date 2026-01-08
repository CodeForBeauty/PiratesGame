using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFunctions : MonoBehaviour
{
    [SerializeField] private UnityClient _client;
    public ProfileData ProfileObject;

    public bool IsSelf { get; set; } = false;

    public void CloseGame()
    {
        Application.Quit();
    }

    public void RecieveData(string json)
    {
        SentPlayerData data = JsonUtility.FromJson<SentPlayerData>(json);

        ProfileObject.LoadProfile(data.Name, $"{data.Country} - {data.City}", data.Rating.ToString(), data.Name != _client.CurrentName);

        if (data.Friends.Length > 0)
        {
            ProfileObject.LoadFriends(data.Friends);
        }
    }
}
