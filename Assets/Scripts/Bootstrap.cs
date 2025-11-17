using UnityEngine;

namespace Kdevaulo.Match3
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private RectTransform _chipsContainer;
        [SerializeField] private GridSettings _gridSettings;

        private ChipView[,] _chips;

        private void Start()
        {
            SpawnGrid();
        }

        private void SpawnGrid()
        {
            Vector2Int size = _gridSettings.GridSize;
            float cellSize = _gridSettings.CellSize;
            Vector2 cellSizeVec = new Vector2(cellSize, cellSize);

            _chips = new ChipView[size.x, size.y];

            float gridWidth = size.x * cellSize;
            float gridHeight = size.y * cellSize;

            float startX = -gridWidth / 2f + cellSize / 2f;
            float startY = -gridHeight / 2f + cellSize / 2f;

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    ChipView chip = Instantiate(_gridSettings.ChipViewPrefab, _chipsContainer);
                    _chips[x, y] = chip;

                    RectTransform rt = chip.RectTransform;

                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);

                    rt.sizeDelta = cellSizeVec;

                    rt.anchoredPosition = new Vector2(
                        startX + x * cellSize,
                        startY + y * cellSize
                    );

                    var sprite = _gridSettings.GetRandomSprite();
                    chip.SetSprite(sprite);
                }
            }
        }
    }
}