using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipLogic : MonoBehaviour
{
    public float MoveTime = 1;

    private bool _isMoving = false;
    private Vector3 _startPos;
    private Vector3 _endPos;
    private float _time = 0;


    void Update()
    {
        if (_isMoving) {
            transform.position = Vector3.Lerp(_startPos, _endPos, _time);
            _time += Time.deltaTime / MoveTime;
        }
    }

    public IEnumerator MoveTo(Vector3 start, Vector3 pos) {
        _startPos = start;
        _endPos = pos;
        _isMoving = true;
        _time = 0;

        yield return new WaitForSeconds(MoveTime);

        _isMoving = false;
    }
}
