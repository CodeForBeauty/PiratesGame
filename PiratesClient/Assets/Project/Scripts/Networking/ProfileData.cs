using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProfileData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameField;
    [SerializeField] private TextMeshProUGUI _addressField;
    [SerializeField] private TextMeshProUGUI _ratingField;
    [SerializeField] private GameObject _addFriend;
    [SerializeField] private GameObject _logout;

    [SerializeField] private GameObject _friendPrefab;
    [SerializeField] private Transform _friendPos;
    [SerializeField] private Vector3 _friendOffset;


    public void LoadProfile(string name, string address, string rating, bool notSelf)
    {
        _nameField.text = name;
        _addressField.text = address;
        _ratingField.text = rating;

        if (_addFriend != null )
            _addFriend.SetActive(notSelf);
        if (_logout != null )
            _logout.SetActive(!notSelf);

        gameObject.SetActive(true);
        if (_friendPrefab == null)
            return;
        for (int i = _friendPos.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_friendPos.transform.GetChild(i).gameObject);
        }
    }

    public void LoadFriends(PublicData[] friends)
    {
        for (int i = 0; i < friends.Length; i++)
        {
            GameObject obj = Instantiate(_friendPrefab, _friendPos.position + (_friendOffset * i), Quaternion.identity, _friendPos);

            obj.GetComponent<ProfileData>().LoadProfile(friends[i].Name, $"{friends[i].Country} - {friends[i].City}", friends[i].Rating.ToString(), true);
        }
    }
}
