using System;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Kdevaulo.Match3
{
    public class ChipInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public event Action<ChipInput, Vector2Int> SwapRequested;

        [SerializeField] private float _minDragDistance = 20f;

        private InputBlocker _inputBlocker;

        private Vector2 _startScreenPosition;
        private bool _dragStarted;

        public void Initialize(InputBlocker inputBlocker)
        {
            if (_inputBlocker == inputBlocker)
                return;

            if (_inputBlocker != null)
                _inputBlocker.Unregister(this);

            _inputBlocker = inputBlocker;

            if (isActiveAndEnabled && _inputBlocker != null)
                _inputBlocker.Register(this);
        }

        public void ResetInputStateFromBlocker()
        {
            _dragStarted = false;
            _startScreenPosition = default;
        }

        private void OnEnable()
        {
            if (_inputBlocker != null)
                _inputBlocker.Register(this);
        }

        private void OnDisable()
        {
            if (_inputBlocker != null)
                _inputBlocker.Unregister(this);

            ResetInputStateFromBlocker();
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (_inputBlocker != null && !_inputBlocker.IsInputEnabled)
            {
                eventData.Use();
                return;
            }

            _dragStarted = true;
            _startScreenPosition = eventData.position;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (_inputBlocker != null && !_inputBlocker.IsInputEnabled)
            {
                eventData.Use();
                return;
            }

            if (!_dragStarted)
                return;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (_inputBlocker != null && !_inputBlocker.IsInputEnabled)
            {
                eventData.Use();
                return;
            }

            if (!_dragStarted)
                return;

            _dragStarted = false;

            var endPosition = eventData.position;
            var delta = endPosition - _startScreenPosition;

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