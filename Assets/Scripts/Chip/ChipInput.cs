using System;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Kdevaulo.Match3
{
    public class ChipInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public event Action<ChipInput, Vector2Int> SwapRequested;

        [SerializeField] private float _minDragDistance = 20f;

        private Vector2 _startScreenPosition;
        private bool _dragStarted;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _dragStarted = true;
            _startScreenPosition = eventData.position;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (!_dragStarted)
                return;

            _dragStarted = false;

            var delta = eventData.position - _startScreenPosition;

            if (delta.sqrMagnitude < _minDragDistance * _minDragDistance)
                return;

            Vector2Int direction;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                direction = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                direction = delta.y > 0 ? Vector2Int.up : Vector2Int.down;

            SwapRequested?.Invoke(this, direction);
        }
    }
}