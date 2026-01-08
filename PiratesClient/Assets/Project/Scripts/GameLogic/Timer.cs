using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timerText;
    public float CurrentTime = 0;

    private bool _isActive = true;

    void Update()
    {
        if (!_isActive)
            return;
        CurrentTime -= Time.deltaTime;
        if (CurrentTime < 0)
        {
            CurrentTime = 0;
        }

        _timerText.text = $"{(int)CurrentTime / 60}:{(int)CurrentTime % 60}";
    }

    public void Pause() {
        _isActive = false;
    }

    public void Resume() {
        _isActive = true;
    }
}
