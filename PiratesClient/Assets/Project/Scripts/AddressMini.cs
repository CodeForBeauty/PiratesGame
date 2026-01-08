using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AddressMini : MonoBehaviour
{
    public Address AddInput;
    public TextMeshProUGUI Text;

    public void OnPressed()
    {
        AddInput._onValueSelected.Invoke(Text);
    }
}
