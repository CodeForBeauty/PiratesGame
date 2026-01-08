using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchEndScreen : MonoBehaviour
{
    [SerializeField] private GameObject _profilePrefab;
    public List<GameEndProfile> Profiles = new();

    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Vector3 _spawnOffset;


    public void LoadProfiles(string[] names, int[] ratings)
    {
        gameObject.SetActive(true);

        for (int i = _spawnPoint.childCount - 1; i >= 0; i--)
        {
            Destroy(_spawnPoint.GetChild(i).gameObject);
        }

        for (int i = 0; i < names.Length; i++)
        {
            GameObject obj = Instantiate(_profilePrefab, _spawnPoint.position + _spawnOffset * i, Quaternion.identity, _spawnPoint);
            GameEndProfile profile = obj.GetComponent<GameEndProfile>();
            Profiles.Add(profile);
            profile.SetData(names[i], ratings[i]);
        }
    }
}
