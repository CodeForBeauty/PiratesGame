using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroPlayer : MonoBehaviour
{
    [SerializeField] private VideoPlayer _video;
    [SerializeField] private string _videoName;

    void Start()
    {
        _video.url = System.IO.Path.Combine(Application.streamingAssetsPath, _videoName);

        _video.loopPointReached += (v) => { SceneManager.LoadScene(1); };

        _video.Prepare();
        _video.prepareCompleted += (v) => { _video.Play(); };
        
    }
}
