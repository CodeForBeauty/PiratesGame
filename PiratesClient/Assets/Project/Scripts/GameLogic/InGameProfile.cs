using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameProfile : MonoBehaviour
{
    [SerializeField] private Button _dinamitesButton;
    [SerializeField] private TextMeshProUGUI _dinamitesText;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private Button _canonsButton;
    [SerializeField] private TextMeshProUGUI _canonsText;

    [SerializeField] private GameObject[] _miniGameResults;
    [SerializeField] private float _resultShowTime = 5;
    private int _gold = 0;
    private int _dinamites = 1;
    private int _canons = 1;


    public void ResetData() {
        _gold = 0;
        _dinamites = 1;
        _canons = 1;
        _goldText.text = _gold.ToString();
        _dinamitesText.text = _dinamites.ToString();
        _canonsText.text = _canons.ToString();
        _canonsButton.interactable = _canons > 0;
    }

    public void AddGold(int amount)
    {
        _gold += amount;
        _goldText.text = _gold.ToString();
    }

    public void AddDinamite(int amount)
    {
        _dinamites += amount;
        _dinamitesText.text = _dinamites.ToString();
    }

    public void SetEnableDinamite(bool value)
    {
        _dinamitesButton.interactable = value;
    }

    public void UseCanon() {
        _canons--;
        _canonsText.text = _canons.ToString();
        _canonsButton.interactable = _canons > 0;
    }

    public void ShowMiniGameResult(int index) {
        _miniGameResults[index].SetActive(true);

        Invoke(nameof(HideAllMiniGame), _resultShowTime);
    }

    private void HideAllMiniGame() {
        foreach (GameObject res in _miniGameResults) {
            res.SetActive(false);
        }
    }
}
