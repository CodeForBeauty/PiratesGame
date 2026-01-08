using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Settings : MonoBehaviour
{
    [SerializeField] private UnityClient _client;
    [SerializeField] private TMP_InputField _name;
    [SerializeField] private TMP_InputField _country;
    [SerializeField] private TMP_InputField _city;


    public void LoadData()
    {
        _name.text = _client.CurrentName;
        _country.text = _client.CurrentCountry;
        _city.text = _client.CurrentCity;
    }

    public void SendData()
    {
        ReceivedData data = new ReceivedData() { PlayerCommand = Command.ChangeData, Name = _name.text, Country = _country.text, City = _city.text };

        _client.SendSearchRequest(data);
    }

    public void SendName() {
        ReceivedData data = new ReceivedData() { PlayerCommand = Command.ChangeData, Name = _name.text };

        _client.SendSearchRequest(data);
    }

    public void SendAdress() {
        ReceivedData data = new ReceivedData() { PlayerCommand = Command.ChangeData, Country = _country.text, City = _city.text };

        _client.SendSearchRequest(data);
    }
}
