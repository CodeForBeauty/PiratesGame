using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileLogic : MonoBehaviour, IPointerClickHandler
{
    public Grid grid;
    public Int2 position;


    public void OnPointerClick(PointerEventData eventData)
    {
        grid.SendMoveCommand(position);
    }
}
