using UnityEngine;

namespace Kdevaulo.Match3
{
    public class ChipController
    {
        private readonly RectTransform _chipsContainer;
        private readonly GridSettings _gridSettings;
        private readonly ChipSettings _chipsSettings;
        private readonly GridModel _gridModel;

        private ChipView[,] _chipViewColection;

        public ChipController(RectTransform chipsContainer, GridSettings gridSettings, ChipSettings chipsSettings,
            GridModel gridModel)
        {
            _chipsContainer = chipsContainer;
            _chipsSettings = chipsSettings;
            _gridSettings = gridSettings;
            _gridModel = gridModel;

            var gridSize = _gridSettings.GridSize;
            _chipViewColection = new ChipView[gridSize.x, gridSize.y];
        }

        public void SpawnGrid()
        {
            var gridSize = _gridSettings.GridSize;

            var cellSize = _gridSettings.CellSize;
            var cellSizeVec = new Vector2(cellSize, cellSize);

            var totalWidth = gridSize.x * cellSize;
            var totalHeight = gridSize.y * cellSize;

            var startX = -totalWidth * 0.5f + cellSize * 0.5f;
            var startY = -totalHeight * 0.5f + cellSize * 0.5f;

            for (var x = 0; x < gridSize.x; x++)
            {
                for (var y = 0; y < gridSize.y; y++)
                {
                    var chip = Object.Instantiate(_chipsSettings.ChipViewPrefab, _chipsContainer);
                    _chipViewColection[x, y] = chip;

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
            }
        }
    }
}