using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InvitedRooms : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _playersPos;
    [SerializeField] private Vector3 _playersOffset;


    public void ReceiveData(string json)
    {
        SentNotificationns data = JsonUtility.FromJson<SentNotificationns>(json);

        for (int i = _playersPos.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_playersPos.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < data.Names.Length; i++)
        {
            GameObject obj = Instantiate(_playerPrefab, _playersPos.position + (_playersOffset * i), Quaternion.identity, _playersPos);

            obj.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = data.Names[i];
        }
    }
}
