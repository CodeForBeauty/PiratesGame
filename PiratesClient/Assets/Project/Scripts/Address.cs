using Mono.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Address : MonoBehaviour
{
    public static string DatabaseName = "cities.sqlite3";

    private SqliteConnection _connection;
    private string _databasePath;


    [SerializeField] public string _currentColumn = "country";
    [SerializeField] private Address _dependent;

    [SerializeField] public UnityEvent<TextMeshProUGUI> _onValueSelected;

    [SerializeField] public TMP_InputField _input;

    [SerializeField] private GameObject _searchPrefab;
    [SerializeField] private Transform _spawnPos;

    [SerializeField] private Vector3 _posOffset;


    private void Awake()
    {
        _databasePath = Path.Combine(Application.streamingAssetsPath, DatabaseName);
        
        _connection = new SqliteConnection($"URI=file:{_databasePath}");
        _connection.Open();

        /*if (_created) {
            return;
        }
        SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "CREATE VIRTUAL TABLE countrySearch USING fts5(content=\"cities\", country, city);";
        command.ExecuteNonQuery();
        _created = true;*/
    }

    private void OnDestroy()
    {
        _connection.Close();
    }

    public void UpdateSearch()
    {
        try {

        Clear();
        if (!_input.isFocused)
            return;

        SqliteCommand command = _connection.CreateCommand();
        if (_dependent != null )
            command.CommandText = $"SELECT DISTINCT {_currentColumn} FROM cities WHERE {_currentColumn} LIKE '%{_input.text.ToLower()}%' AND {_dependent._currentColumn} LIKE '%{_dependent._input.text.ToLower()}%' LIMIT 20";
        else
            command.CommandText = $"SELECT DISTINCT {_currentColumn} FROM cities WHERE {_currentColumn} LIKE '%{_input.text.ToLower()}%' LIMIT 20";

        SqliteDataReader reader = command.ExecuteReader();
        int j = 0;
        while (reader.Read())
        {
            string value = reader.GetString(0);
            GameObject obj = Instantiate(_searchPrefab, _spawnPos.position + (_posOffset * j), Quaternion.identity, _spawnPos);

            obj.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = value;
            obj.GetComponent<AddressMini>().AddInput = this;

            j++;
        }
        }
        catch {

        }
    }

    public void SelectValue(TextMeshProUGUI value)
    {
        SqliteCommand command = _connection.CreateCommand();
        command.CommandText = $"SELECT DISTINCT {_currentColumn} FROM search WHERE {_currentColumn} LIKE '%{value.text.ToLower()}%' LIMIT 20";

        string found = (string)command.ExecuteScalar();

        _input.text = found;
        Clear();
    }

    public void SelectFromOther(Address other)
    {
        SqliteCommand command = _connection.CreateCommand();
        command.CommandText = $"SELECT {_currentColumn} FROM search WHERE {other._currentColumn} LIKE '%{other._input.text.ToLower()}%' LIMIT 20";

        string found = (string)command.ExecuteScalar();

        _input.text = found;
        Clear();
    }

    public void Clear()
    {
        for (int i = _spawnPos.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_spawnPos.transform.GetChild(i).gameObject);
        }
    }

    public void ClearAfterTime(float time)
    {
        Invoke(nameof(Clear), time);
    }
}
