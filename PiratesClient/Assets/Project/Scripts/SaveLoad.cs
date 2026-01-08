using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;

public class SaveLoad : MonoBehaviour
{
    [SerializeField] private UnityEvent _onLoaded;

    [SerializeField] private TMP_InputField _login;
    [SerializeField] private TMP_InputField _password;


    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[PlayerPrefs.GetInt("locale")];

        if (PlayerPrefs.HasKey("login") && PlayerPrefs.HasKey("password"))
        {
            _login.text = PlayerPrefs.GetString("login");
            _password.text = PlayerPrefs.GetString("password");
            _onLoaded.Invoke();
        }
    }

    public void SaveData()
    {
        PlayerPrefs.SetString("login", _login.text);
        PlayerPrefs.SetString("password", _password.text);
    }

    public void DeleteData() {
        PlayerPrefs.DeleteKey("login");
        PlayerPrefs.DeleteKey("password");
    }

    public void ChangeName(string newName) {
        PlayerPrefs.SetString("login", newName);
    }
}
