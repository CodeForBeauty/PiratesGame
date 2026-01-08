using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource _menuMusic;
    [SerializeField] private AudioSource _gameMusic;

    [SerializeField] private float _interpolationTime = 1.0f;

    public float _volume;

    [SerializeField] private Slider _volumeControl;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("volume"))
        {
            SetVolume(PlayerPrefs.GetFloat("volume"));
        }
    }

    void Start()
    {
        _gameMusic.Stop();
        _menuMusic.Play();
    }

    public void SetVolume(float value) {
        _menuMusic.volume = value;
        _gameMusic.volume = value;
        _volume = value;
        Grid.Volume = value;
        _volumeControl.value = value;
        PlayerPrefs.SetFloat("volume", value);
    }

    public void PlayMenuMusic()
    {
        StartCoroutine(Interpolate(_menuMusic, _gameMusic));
    }

    public void PlayGameMusic()
    {
        StartCoroutine(Interpolate(_gameMusic, _menuMusic));
    }

    private IEnumerator Interpolate(AudioSource first, AudioSource second)
    {
        first.Play();
        int frames = 30;
        for (int i = 0; i < frames * _interpolationTime; i++)
        {
            first.volume = i / (frames * _interpolationTime) * _volume;
            second.volume = ((float)frames - i) / (frames * _interpolationTime) * _volume;
            yield return new WaitForSeconds(1 / frames);
        }
        second.Stop();
    }
}
