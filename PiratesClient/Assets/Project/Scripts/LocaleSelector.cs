using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using System;


public class LocaleDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _localesDropdown;

    private IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        int selectedLocale = 0;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[PlayerPrefs.GetInt("locale")];

        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
        {
            Locale locale = LocalizationSettings.AvailableLocales.Locales[i];
            options.Add(new TMP_Dropdown.OptionData(locale.name));
            if (locale == LocalizationSettings.SelectedLocale) selectedLocale = i;
        }

        _localesDropdown.options = options;
        _localesDropdown.value = selectedLocale;
        _localesDropdown.onValueChanged.AddListener(OnLocaleChanged);
    }

    private static void OnLocaleChanged(int index)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        PlayerPrefs.SetInt("locale", index);
        PlayerPrefs.Save();
    }
}