using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Notifications : MonoBehaviour
{
    [SerializeField] private GameObject _notificationPrefab;
    [SerializeField] private Transform _notificationsSpawn;
    [SerializeField] private Vector3 _positionsOffset;


    public void ReceiveNotifications(string json)
    {
        SentNotificationns notifications = JsonUtility.FromJson<SentNotificationns>(json);

        for (int i = _notificationsSpawn.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_notificationsSpawn.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < notifications.Names.Length; i++)
        {
            GameObject obj = Instantiate(_notificationPrefab, _notificationsSpawn.position + (_positionsOffset * i), Quaternion.identity, _notificationsSpawn);

            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = notifications.Names[i];
            
        }
    }

    public void RebuildNotifications(GameObject notification)
    {
        Destroy(notification);

        for (int i = _notificationsSpawn.transform.childCount - 1; i >= 0; i--)
        {
            _notificationsSpawn.transform.GetChild(i).position = _notificationsSpawn.position + (_positionsOffset * i);
        }
    }
}
