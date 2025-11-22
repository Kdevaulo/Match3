using UnityEngine;
using UnityEngine.EventSystems;

namespace Kdevaulo.Match3
{
    public class ChipInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private float _minDragDistance = 20f;

        [SerializeField] private ChipView _chipView;

        private ChipController _chipController;
        private GridModel _gridModel;

        private Vector2 _startScreenPosition;

        private bool _initialized;
        private bool _dragStarted;

        public void Initialize(ChipController chipController, GridModel gridModel)
        {
            _chipController = chipController;
            _gridModel = gridModel;
            _initialized = true;
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _dragStarted = false;

            if (!_initialized || _chipView == null || _gridModel == null)
                return;

            var cell = _chipView.Cell;

            if (!IsInsideBounds(cell))
                return;

            if (_gridModel.GetChip(cell.x, cell.y) == Chip.None)
                return;

            _startScreenPosition = eventData.position;
            _dragStarted = true;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (!_dragStarted || !_initialized || _chipView == null || _gridModel == null)
                return;

            _dragStarted = false;

            var startCell = _chipView.Cell;

            if (!IsInsideBounds(startCell))
                return;

            var delta = eventData.position - _startScreenPosition;

            if (delta.sqrMagnitude < _minDragDistance * _minDragDistance)
                return;

            Vector2Int direction;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                direction = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                direction = delta.y > 0 ? Vector2Int.up : Vector2Int.down;

            var targetCell = startCell + direction;

            if (!IsInsideBounds(targetCell))
                return;

            _chipController.Swap(startCell, targetCell);
        }

        private bool IsInsideBounds(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < _gridModel.Width &&
                   cell.y >= 0 && cell.y < _gridModel.Height;
        }
    }
}