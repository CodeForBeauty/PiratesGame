using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameEndProfile : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _rating;

    [SerializeField] private Color _downColor = Color.red;
    [SerializeField] private Color _upColor = Color.green;


    public void SetData(string name, int rating)
    {
        _rating.color = rating < 0 ? _downColor : _upColor;

        _name.text = name;
        _rating.text = rating.ToString();
    }
}
