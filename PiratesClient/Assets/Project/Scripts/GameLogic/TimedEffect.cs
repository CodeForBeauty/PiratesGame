using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedEffect : MonoBehaviour
{
    [SerializeField] public float _lifetime = 1.0f;

    void Start()
    {
        Invoke(nameof(Destroy), _lifetime);
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
