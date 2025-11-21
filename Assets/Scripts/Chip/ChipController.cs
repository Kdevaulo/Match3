using System.Collections.Generic;

using UnityEngine;

namespace Kdevaulo.Match3
{
    public class ChipController
    {
        private readonly RectTransform _chipsContainer;
        private readonly GridSettings _gridSettings;
        private readonly ChipSettings _chipsSettings;
        private readonly GridModel _gridModel;

        private readonly ChipView[,] _chipViewCollection;
        private readonly List<Vector2Int> _spawnedCells = new List<Vector2Int>();

        public ChipController(RectTransform chipsContainer, GridSettings gridSettings, ChipSettings chipsSettings,
            GridModel gridModel)
        {
            _chipsContainer = chipsContainer;
            _chipsSettings = chipsSettings;
            _gridSettings = gridSettings;
            _gridModel = gridModel;

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
                    CreateChipView(x, y, cellSize, startX, startY);
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

        public void ApplyGravity()
        {
            GetGridGeometry(out var width, out var height, out var cellSize, out var startX, out var startY);

            for (var x = 0; x < width; x++)
            {
                var targetY = 0;

                for (var y = 0; y < height; y++)
                {
                    var type = _gridModel.GetChip(x, y);

                    if (type == Chip.None)
                        continue;

                    MoveChip(x, y, startX, startY, targetY, type, cellSize);

                    targetY++;
                }
            }
        }

        public void SpawnMissingChipsAboveGrid()
        {
            _spawnedCells.Clear();

            GetGridGeometry(out var width, out var height, out var cellSize, out var startX, out var startY);

            for (var x = 0; x < width; x++)
            {
                var emptyCount = 0;

                for (var y = height - 1; y >= 0; y--)
                {
                    if (_gridModel.GetChip(x, y) == Chip.None)
                        emptyCount++;
                    else
                        break;
                }

                for (var i = 0; i < emptyCount; i++)
                {
                    var y = height - emptyCount + i;

                    var chip = Object.Instantiate(_chipsSettings.ChipViewPrefab, _chipsContainer);
                    _chipViewCollection[x, y] = chip;

                    var rt = chip.RectTransform;
                    rt.sizeDelta = new Vector2(cellSize, cellSize);

                    var posX = startX + x * cellSize;
                    var posY = startY + (height + 1 + i) * cellSize;
                    rt.anchoredPosition = new Vector2(posX, posY);

                    var type = _chipsSettings.GetRandomType();
                    _gridModel.SetChip(x, y, type);

                    var sprite = _chipsSettings.GetSprite(type);
                    chip.SetSprite(sprite);

                    _spawnedCells.Add(new Vector2Int(x, y));
                }
            }
        }

        public void MoveSpawnedChipsDown()
        {
            if (_spawnedCells.Count == 0)
                return;

            GetGridGeometry(out _, out _, out var cellSize, out var startX, out var startY);

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

        private void GetGridGeometry(out int width, out int height, out float cellSize,
            out float startX, out float startY)
        {
            var gridSize = _gridSettings.GridSize;
            width = gridSize.x;
            height = gridSize.y;

            cellSize = _gridSettings.CellSize;

            var totalWidth = width * cellSize;
            var totalHeight = height * cellSize;

            startX = -totalWidth * 0.5f + cellSize * 0.5f;
            startY = -totalHeight * 0.5f + cellSize * 0.5f;
        }

        private void MoveChip(int x, int y, float startX, float startY, int targetY, Chip type, float cellSize)
        {
            var view = _chipViewCollection[x, y];

            if (y != targetY)
            {
                _gridModel.SetChip(x, targetY, type);
                _gridModel.SetChip(x, y, Chip.None);

                _chipViewCollection[x, targetY] = view;
                _chipViewCollection[x, y] = null;

                if (view != null)
                {
                    MoveChipDown(view.RectTransform, startX, x, cellSize, startY, targetY);
                }
            }
        }

        private void CreateChipView(int x, int y, float cellSize, float startX, float startY)
        {
            var cellSizeVec = new Vector2(cellSize, cellSize);

            var chip = Object.Instantiate(_chipsSettings.ChipViewPrefab, _chipsContainer);
            _chipViewCollection[x, y] = chip;

            var rt = chip.RectTransform;
            rt.sizeDelta = cellSizeVec;

            rt.anchoredPosition = new Vector2(
                startX + x * cellSize,
                startY + y * cellSize
            );

            var type = _chipsSettings.GetRandomType();
            _gridModel.SetChip(x, y, type);

            var sprite = _chipsSettings.GetSprite(type);
            chip.SetSprite(sprite);
        }

        private void MoveChipDown(RectTransform rt, float startX, int x, float cellSize, float startY, int targetY)
        {
            rt.anchoredPosition = new Vector2(
                startX + x * cellSize,
                startY + targetY * cellSize
            );
        }
    }
}