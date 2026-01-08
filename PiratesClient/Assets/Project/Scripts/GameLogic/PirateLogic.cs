using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PirateLogic : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private AudioClip _movingSound;
    [SerializeField] private SpriteRenderer _top;

    public Grid grid;
    public int Player = -1;
    public int ThisPirate = -1;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private float _time = 0;
    private bool _isMoving = false;

    public float MoveTime = 1;

    public Vector3 SameTileOffset;
    private Vector3 _offset;

    public SpriteRenderer Renderer;
    public GameObject GoldSprite;

    private void Awake()
    {
        Renderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (_isMoving)
        {
            transform.position = Vector3.Lerp(_startPos, _endPos, _time);
            _time += Time.deltaTime / MoveTime;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (grid.SelectedPirate == ThisPirate)
        {
            grid.SelectedPirate = ThisPirate;
            grid.DeselectPirates();
            grid.DisableTiles();
        }
        if (grid.SelectedPirate != -1)
        {
            if (Math.Abs(grid.Pirates[Player][ThisPirate].Position.x - grid.Pirates[Player][grid.SelectedPirate].Position.x) <= 1 &&
                Math.Abs(grid.Pirates[Player][ThisPirate].Position.y - grid.Pirates[Player][grid.SelectedPirate].Position.y) <= 1)
            {
                grid.SendMoveCommand(grid.Pirates[Player][ThisPirate].Position);
                return;
            }
        }
        grid.SelectPirate(Player, ThisPirate);
    }

    public IEnumerator MoveTo(Vector3 start, Vector3 pos)
    {
        _startPos = start + _offset;
        _endPos = pos + _offset;
        _isMoving = true;
        _time = 0;

        //AudioSource.PlayClipAtPoint(_movingSound, Camera.main.transform.position);

        yield return new WaitForSeconds(MoveTime);

        _isMoving = false;
    }

    public void Initialize(Color col)
    {
        _offset.x = (ThisPirate - ((float)grid.Pirates[Player].Count / 2)) * SameTileOffset.x;
        _offset.y = (Player - ((float)grid.Ships.Count / 2)) * SameTileOffset.y;

        transform.position = transform.position + _offset;

        _top.color = col;
    }
}
