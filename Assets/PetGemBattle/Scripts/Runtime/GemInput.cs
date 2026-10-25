using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PetGemBattle
{
    public sealed class GemInput : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Cell Cell;
        public Action<Cell> Click;
        public Action<Cell, Cell> Drag;
        private bool dragged;
        private UnityEngine.UI.Outline hover;
        public void OnPointerEnter(PointerEventData e)
        {
            if(hover==null){hover=gameObject.AddComponent<UnityEngine.UI.Outline>();hover.effectColor=new Color(1,.9f,.5f);hover.effectDistance=new Vector2(2,2);}
            hover.enabled=true;
        }
        public void OnPointerExit(PointerEventData e){if(hover!=null)hover.enabled=false;}
        public void OnPointerDown(PointerEventData e) { dragged=false; }
        public void OnPointerClick(PointerEventData e) { if (!dragged) Click?.Invoke(Cell); dragged=false; }
        public void OnBeginDrag(PointerEventData e) { dragged=true; }
        public void OnDrag(PointerEventData e) { }
        public void OnEndDrag(PointerEventData e)
        {
            var rect = (RectTransform)transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect.parent as RectTransform,e.pressPosition,e.pressEventCamera,out var start);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect.parent as RectTransform,e.position,e.pressEventCamera,out var end);
            var delta = end-start;
            if (delta.magnitude < 18f) return;
            int dx=0,dy=0;
            if (Mathf.Abs(delta.x)>Mathf.Abs(delta.y)) dx=delta.x>0 ? 1 : -1;
            else dy=delta.y>0 ? 1 : -1;
            Drag?.Invoke(Cell,new Cell(Cell.X+dx,Cell.Y+dy));
        }
    }
}
