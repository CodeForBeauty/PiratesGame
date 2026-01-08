using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Ratings : MonoBehaviour
{
    [SerializeField] private UnityClient _client;

    [SerializeField] private GameObject _profilePrefab;
    [SerializeField] private GameObject _spawnPoint;
    [SerializeField] private Vector3 _spawnOffset;

    [SerializeField] private TMP_InputField _name;
    [SerializeField] private TMP_InputField _country;
    [SerializeField] private TMP_InputField _city;



    public void SendSearchRequest()
    {
        ReceivedData data = new() { PlayerCommand = Command.Search, Name = _name.text, Country = _country.text, City = _city.text };

        _client.SendSearchRequest(data);
    }

    public void ReceiveSearchResult(string json)
    {
        SentSearchResult data = JsonUtility.FromJson<SentSearchResult>(json);

        for (int i = _spawnPoint.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_spawnPoint.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < data.Results.Length; i++)
        {
            GameObject obj = Instantiate(_profilePrefab, _spawnPoint.transform.position + (_spawnOffset * i), Quaternion.identity, _spawnPoint.transform);

            obj.GetComponent<ProfileData>().LoadProfile(data.Results[i].Name, $"{data.Results[i].Country} - {data.Results[i].City}", data.Results[i].Rating.ToString(), true);
        }
    }
}
