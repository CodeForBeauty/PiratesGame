using Microsoft.Data.Sqlite;
using System.Data;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.RegularExpressions;


public static class SaveLoadSystem
{
    readonly public static string PlayersDataPath = "database/players.sqlite3";
    readonly public static string FriendsDataPath = "database/friends.sqlite3";
    readonly public static string SavePath = "Players/";

    private static SqliteConnection? _connection;
    private static SqliteConnection? _friendsConn;


    public static void Initialize()
    {
        _connection = CreateConnection(PlayersDataPath);
        _friendsConn = CreateConnection(FriendsDataPath);
    }

    public static SqliteConnection CreateConnection(string path)
    {
        SqliteConnection sqlite_conn;
        sqlite_conn = new SqliteConnection($"Data Source = {path}; Mode = ReadWrite; Default Timeout = 1000; Cache = Shared");
        try
        {
            sqlite_conn.Open();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return sqlite_conn;
    }

    public static void CloseConnection()
    {
        _connection?.Close();
    }

    public static bool SaveData(PlayerData data)
    {
        if (_connection == null || string.IsNullOrEmpty(data.Name))
            return false;

        SqliteDataReader sqlite_datareader;
        SqliteCommand sqlite_cmd;
        sqlite_cmd = _connection.CreateCommand();
        sqlite_cmd.CommandText = $"SELECT * FROM players WHERE name = '{data.Name}'";

        sqlite_datareader = sqlite_cmd.ExecuteReader();
        if (!sqlite_datareader.Read())
        {
            return false;
        }
        sqlite_datareader.Close();

        sqlite_cmd = _connection.CreateCommand();
        sqlite_cmd.CommandText = $"UPDATE players SET rating = {data.Rating}, country = '{data.Country}', city = '{data.City}' WHERE name = '{data.Name}'; ";
        sqlite_cmd.ExecuteNonQuery();

        return true;
    }

    public static bool LoadData(string playerName, out PlayerData? data)
    {
        if (_connection == null || string.IsNullOrEmpty(playerName))
        {
            data = null;
            return false;
        }
        SqliteDataReader sqlite_datareader;
        SqliteCommand sqlite_cmd;
        sqlite_cmd = _connection.CreateCommand();
        sqlite_cmd.CommandText = $"SELECT name, password, rating, country, city FROM players WHERE name = '{playerName}'";

        sqlite_datareader = sqlite_cmd.ExecuteReader();
        if (sqlite_datareader.Read())
        {
            data = new PlayerData();
            data.Name = sqlite_datareader.GetString(0);
            data.Password = sqlite_datareader.GetString(1);
            data.Rating = sqlite_datareader.GetInt32(2);
            data.Country = sqlite_datareader.GetString(3);
            data.City = sqlite_datareader.GetString(4);

            data.Friends = GetFriends(playerName);

            return true;
        }
        sqlite_datareader.Close();

        data = null;
        return false;
    }

    public static bool CreateData(PlayerData data)
    {
        if (_connection == null || string.IsNullOrEmpty(data.Name) || string.IsNullOrEmpty(data.Password) || data.Name.Length > 10 || data.Password.Length > 12)
        {
            return false;
        }

        SqliteCommand sqlite_cmd;
        sqlite_cmd = _connection.CreateCommand();
        sqlite_cmd.CommandText = $"SELECT count(id) FROM players WHERE name = '{data.Name}'";

        sqlite_cmd.CommandType = CommandType.Text;
        int RowCount = Convert.ToInt32(sqlite_cmd.ExecuteScalar());

        if (RowCount > 0)
        {
            return false;
        }

        sqlite_cmd.CommandText = $"INSERT INTO players (name, password, rating, country, city) VALUES('{data.Name}', '{data.Password}', '{data.Rating}', '{data.Country}', '{data.City}'); ";
        sqlite_cmd.ExecuteNonQuery();

        return true;
    }

    public static PublicData[]? GetFriends(string player)
    {
        if (_friendsConn == null || string.IsNullOrEmpty(player))
        {
            return null;
        }

        List<PublicData> friends = [];

        SqliteDataReader sqlite_datareader;
        SqliteCommand sqlite_cmd;
        sqlite_cmd = _friendsConn.CreateCommand();

        sqlite_cmd.CommandText = $"SELECT second FROM friends WHERE first = '{player}'";
        sqlite_datareader = sqlite_cmd.ExecuteReader();
        if (sqlite_datareader.Read())
        {
            friends.Add(GetPlayerData(sqlite_datareader.GetString(0)));
        }
        sqlite_datareader.Close();

        sqlite_cmd.CommandText = $"SELECT first FROM friends WHERE second = '{player}'";
        sqlite_datareader = sqlite_cmd.ExecuteReader();
        if (sqlite_datareader.Read())
        {
            friends.Add(GetPlayerData(sqlite_datareader.GetString(0)));
        }
        sqlite_datareader.Close();

        return friends.ToArray();
    }

    public static PublicData GetPlayerData(string name)
    {
        if (_connection == null || string.IsNullOrEmpty(name))
        {
            return new PublicData();
        }

        PublicData found = new();

        SqliteDataReader sqlite_datareader;
        SqliteCommand sqlite_cmd;
        sqlite_cmd = _connection.CreateCommand();

        sqlite_cmd.CommandText = $"SELECT name, rating, country, city FROM players WHERE name = '{name}'";
        sqlite_datareader = sqlite_cmd.ExecuteReader();
        if (sqlite_datareader.Read())
        {
            found.Name = sqlite_datareader.GetString(0);
            found.Rating = sqlite_datareader.GetInt32(1);
            found.Country = sqlite_datareader.GetString(2);
            found.City = sqlite_datareader.GetString(3);
        }
        sqlite_datareader.Close();

        return found;
    }

    public static PublicData[] SearchPlayers(string query, string country, string city)
    {
        if (_connection == null)
        {
            return [];
        }

        List<PublicData> found = [];

        SqliteDataReader sqlite_datareader;
        SqliteCommand sqlite_cmd;
        sqlite_cmd = _connection.CreateCommand();

        country = Regex.Replace(country.ToLower(), @"(^\w)|(\s\w)", m => m.Value.ToUpper());
        city = Regex.Replace(city.ToLower(), @"(^\w)|(\s\w)", m => m.Value.ToUpper());

        sqlite_cmd.CommandText = $"SELECT name, rating, country, city FROM players WHERE name LIKE '%{query}%' AND country LIKE '%{country}%' AND city LIKE '%{city}%' ORDER BY rating DESC LIMIT 20";
        sqlite_datareader = sqlite_cmd.ExecuteReader();
        while (sqlite_datareader.Read())
        {
            PublicData playerData = new PublicData();
            playerData.Name = sqlite_datareader.GetString(0);
            playerData.Rating = sqlite_datareader.GetInt32(1);
            playerData.Country = sqlite_datareader.GetString(2);
            playerData.City = sqlite_datareader.GetString(3);
            found.Add(playerData);
        }
        sqlite_datareader.Close();

        return found.ToArray();
    }

    public static bool ChangeName(string oldName, string newName)
    {
        if (_connection == null || _friendsConn == null || string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
            return false;

        SqliteDataReader sqlite_datareader;
        SqliteCommand sqlite_cmd;
        sqlite_cmd = _connection.CreateCommand();
        sqlite_cmd.CommandText = $"SELECT * FROM players WHERE name = '{oldName}';";

        sqlite_datareader = sqlite_cmd.ExecuteReader();
        if (!sqlite_datareader.Read())
        {
            return false;
        }
        sqlite_datareader.Close();

        sqlite_cmd = _connection.CreateCommand();
        sqlite_cmd.CommandText = $"UPDATE players SET name = '{newName}' WHERE name = '{oldName}'; ";
        sqlite_cmd.ExecuteNonQuery();

        sqlite_cmd = _friendsConn.CreateCommand();
        sqlite_cmd.CommandText = $"UPDATE friends SET first = '{newName}' WHERE first = '{oldName}'; ";
        sqlite_cmd.ExecuteNonQuery();

        sqlite_cmd.CommandText = $"UPDATE friends SET second = '{newName}' WHERE second = '{oldName}'; ";
        sqlite_cmd.ExecuteNonQuery();

        return true;
    }

    public static void AddFriends(string first, string second)
    {
        if (_friendsConn == null || string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second))
            return;

        SqliteCommand sqlite_cmd;
        sqlite_cmd = _friendsConn.CreateCommand();
        sqlite_cmd.CommandText = $"SELECT count(id) FROM friends WHERE (first = '{first}' AND second = '{second}') OR (second = '{first}' AND first = '{second}')";

        sqlite_cmd.CommandType = CommandType.Text;
        int RowCount = Convert.ToInt32(sqlite_cmd.ExecuteScalar());

        if (RowCount > 0)
        {
            return;
        }

        sqlite_cmd.CommandText = $"INSERT INTO friends (first, second) VALUES('{first}', '{second}'); ";
        sqlite_cmd.ExecuteNonQuery();
    }


    // Old save system
    /*public static bool SaveData(PlayerData data)
    {
        if (string.IsNullOrEmpty(data.Name))
            return false;
        string filePath = Path.Combine(SavePath, data.Name + ".sv");

        FileStream stream = new(filePath, FileMode.Create);
        JsonSerializer.Serialize(stream, data);
        stream.Close();

        return true;
    }

    public static bool LoadData(string playerName, out PlayerData? data)
    {
        string filePath = Path.Combine(SavePath, playerName + ".sv");
        if (!File.Exists(filePath))
        {
            data = new PlayerData();
            return false;
        }

        FileStream stream = new(filePath, FileMode.Open);
        data = JsonSerializer.Deserialize<PlayerData>(stream);
        stream.Close();

        return true;
    }

    public static bool CreateData(PlayerData data)
    {
        string filePath = Path.Combine(SavePath, data.Name + ".sv");
        if (File.Exists(filePath))
            return false;

        FileStream stream = new(filePath, FileMode.Create);
        JsonSerializer.Serialize(stream, data);
        stream.Close();

        return true;
    }*/
}