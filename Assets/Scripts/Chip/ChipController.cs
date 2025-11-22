using System.Collections.Generic;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Kdevaulo.Match3
{
    public class ChipController
    {
        private readonly RectTransform _chipsContainer;
        private readonly ChipSettings _chipsSettings;
        private readonly GridSettings _gridSettings;
        private readonly InputBlocker _inputBlocker;
        private readonly GameEventBus _eventBus;
        private readonly GridModel _gridModel;

        private readonly ChipView[,] _chipViewCollection;
        private readonly List<Vector2Int> _spawnedCells = new List<Vector2Int>();

        public ChipController(RectTransform chipsContainer, GridSettings gridSettings, ChipSettings chipsSettings,
            GridModel gridModel, GameEventBus eventBus, InputBlocker inputBlocker)
        {
            _chipsContainer = chipsContainer;
            _chipsSettings = chipsSettings;
            _gridSettings = gridSettings;
            _inputBlocker = inputBlocker;
            _gridModel = gridModel;
            _eventBus = eventBus;

            var gridSize = _gridSettings.GridSize;
            _chipViewCollection = new ChipView[gridSize.x, gridSize.y];
        }

        public void SpawnGrid()
        {
            var gridSize = _gridSettings.GridSize;
            var cellSize = _gridSettings.CellSize;

            var totalWidth = gridSize.x * cellSize;
            var totalHeight = gridSize.y * cellSize;

            var startX = -totalWidth * 0.5f + cellSize * 0.5f;
            var startY = -totalHeight * 0.5f + cellSize * 0.5f;

            for (var x = 0; x < gridSize.x; x++)
            {
                for (var y = 0; y < gridSize.y; y++)
                {
                    var position = new Vector2(
                        CalculatePosition(startX, x, cellSize),
                        CalculatePosition(startY, y, cellSize));

                    CreateChipView(x, y, cellSize, position);
                }
            }
        }

        public void HighlightCells(IReadOnlyList<Vector2Int> cells, Color color)
        {
            foreach (var cell in cells)
            {
                var view = _chipViewCollection[cell.x, cell.y];

                if (view != null)
                {
                    view.SetColor(color);
                }
            }
        }

        public void ClearCells(IReadOnlyList<Vector2Int> cells)
        {
            foreach (var cell in cells)
            {
                var x = cell.x;
                var y = cell.y;

                var view = _chipViewCollection[x, y];

                if (view != null)
                {
                    Object.Destroy(view.gameObject);
                    _chipViewCollection[x, y] = null;
                }

                _gridModel.SetChip(x, y, Chip.None);
            }
        }

        public void MoveByGravity(List<MoveParams> moveParams)
        {
            if (moveParams == null || moveParams.Count == 0)
                return;

            CalculateGridGeometry(out var cellSize, out var startX, out var startY);

            foreach (var move in moveParams)
            {
                var view = _chipViewCollection[move.FromX, move.FromY];

                _chipViewCollection[move.ToX, move.ToY] = view;
                _chipViewCollection[move.FromX, move.FromY] = null;

                if (view != null)
                {
                    MoveChipToCell(view.RectTransform, startX, cellSize, startY, move.ToX, move.ToY);
                }
            }
        }

        public void SpawnMissingChipsAboveGrid(List<SpawnParams> spawns)
        {
            _spawnedCells.Clear();

            if (spawns == null || spawns.Count == 0)
                return;

            CalculateGridGeometry(out var cellSize, out var startX, out var startY);

            foreach (var spawn in spawns)
            {
                var x = spawn.X;
                var y = spawn.Y;

                var chip = Object.Instantiate(_chipsSettings.ChipViewPrefab, _chipsContainer);
                _chipViewCollection[x, y] = chip;

                var rt = chip.RectTransform;
                rt.sizeDelta = new Vector2(cellSize, cellSize);

                var aboveRow = _gridSettings.GridSize.y + 1 + spawn.OrderFromTop;

                rt.anchoredPosition = new Vector2(
                    CalculatePosition(startX, x, cellSize),
                    CalculatePosition(startY, aboveRow, cellSize));

                var sprite = _chipsSettings.GetSprite(spawn.Type);
                chip.SetSprite(sprite);

                var input = chip.Input;

                if (input != null)
                {
                    input.Initialize(_inputBlocker);
                    input.SwapRequested += OnSwapRequested;
                }

                _spawnedCells.Add(new Vector2Int(x, y));
            }
        }

        public void MoveSpawnedChipsDown()
        {
            if (_spawnedCells.Count == 0)
                return;

            CalculateGridGeometry(out var cellSize, out var startX, out var startY);

            foreach (var cell in _spawnedCells)
            {
                var x = cell.x;
                var y = cell.y;

                var view = _chipViewCollection[x, y];

                if (view == null)
                    continue;

                MoveChipDown(view.RectTransform, startX, x, cellSize, startY, y);
            }

            _spawnedCells.Clear();
        }

        private void CreateChipView(int x, int y, float cellSize, Vector2 position)
        {
            var chip = Object.Instantiate(_chipsSettings.ChipViewPrefab, _chipsContainer);
            _chipViewCollection[x, y] = chip;

            var rt = chip.RectTransform;
            rt.sizeDelta = new Vector2(cellSize, cellSize);
            rt.anchoredPosition = position;

            var type = _chipsSettings.GetRandomType();
            _gridModel.SetChip(x, y, type);

            var sprite = _chipsSettings.GetSprite(type);
            chip.SetSprite(sprite);

            var input = chip.Input;

            if (input != null)
            {
                input.Initialize(_inputBlocker);
                input.SwapRequested += OnSwapRequested;
            }
        }

        private void OnSwapRequested(ChipInput input, Vector2Int direction)
        {
            if (!TryGetCell(input, out var startCell))
                return;

            var width = _gridModel.Width;
            var height = _gridModel.Height;

            if (startCell.x < 0 || startCell.x >= width ||
                startCell.y < 0 || startCell.y >= height)
                return;

            var typeA = _gridModel.GetChip(startCell.x, startCell.y);
            if (typeA == Chip.None)
                return;

            var targetCell = startCell + direction;

            if (targetCell.x < 0 || targetCell.x >= width ||
                targetCell.y < 0 || targetCell.y >= height)
                return;

            var typeB = _gridModel.GetChip(targetCell.x, targetCell.y);
            if (typeA == typeB)
                return;

            Swap(startCell, targetCell);
        }

        private bool TryGetCell(ChipInput input, out Vector2Int cell)
        {
            var width = _gridModel.Width;
            var height = _gridModel.Height;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var view = _chipViewCollection[x, y];

                    if (view != null && view.Input == input)
                    {
                        cell = new Vector2Int(x, y);
                        return true;
                    }
                }
            }

            cell = default;
            return false;
        }

        private void Swap(Vector2Int a, Vector2Int b)
        {
            if (a == b)
                return;

            _gridModel.SwapChips(a.x, a.y, b.x, b.y);

            var viewA = _chipViewCollection[a.x, a.y];
            var viewB = _chipViewCollection[b.x, b.y];

            _chipViewCollection[a.x, a.y] = viewB;
            _chipViewCollection[b.x, b.y] = viewA;

            CalculateGridGeometry(out var cellSize, out var startX, out var startY);

            if (viewA != null)
            {
                MoveChipToCell(viewA.RectTransform, startX, cellSize, startY, b.x, b.y);
            }

            if (viewB != null)
            {
                MoveChipToCell(viewB.RectTransform, startX, cellSize, startY, a.x, a.y);
            }

            _eventBus?.Publish(new PlayerSwapPerformedEvent(a, b));
        }

        private void CalculateGridGeometry(out float cellSize, out float startX, out float startY)
        {
            var gridSize = _gridSettings.GridSize;
            cellSize = _gridSettings.CellSize;

            var totalWidth = gridSize.x * cellSize;
            var totalHeight = gridSize.y * cellSize;

            startX = -totalWidth * 0.5f + cellSize * 0.5f;
            startY = -totalHeight * 0.5f + cellSize * 0.5f;
        }

        private void MoveChipDown(RectTransform rt, float startX, int x, float cellSize, float startY, int targetY)
        {
            MoveChipToCell(rt, startX, cellSize, startY, x, targetY);
        }

        private void MoveChipToCell(RectTransform rt, float startX, float cellSize, float startY, int x, int y)
        {
            rt.anchoredPosition = new Vector2(
                CalculatePosition(startX, x, cellSize),
                CalculatePosition(startY, y, cellSize));
        }

        private float CalculatePosition(float startPosition, int count, float size)
        {
            return startPosition + count * size;
        }
    }
}