using System;

using UnityEngine;

namespace Kdevaulo.Match3
{
    public class GridModel
    {
        private readonly Chip[,] _chips;
        private readonly bool[,] _dirtyCells;
        private readonly bool[] _dirtyRows;
        private readonly bool[] _dirtyColumns;

        public int Width { get; }
        public int Height { get; }

        public bool[] DirtyRows => _dirtyRows;
        public bool[] DirtyColumns => _dirtyColumns;

        public GridModel(Vector2Int gridSize)
        {
            Width = gridSize.x;
            Height = gridSize.y;

            _chips = new Chip[Width, Height];
            _dirtyCells = new bool[Width, Height];
            _dirtyRows = new bool[Height];
            _dirtyColumns = new bool[Width];
        }

        public Chip GetChip(int x, int y)
        {
            return _chips[x, y];
        }

        public void SetChip(int x, int y, Chip type)
        {
            _chips[x, y] = type;
            MarkDirty(x, y);
        }

        public void MarkDirty(int x, int y)
        {
            _dirtyCells[x, y] = true;
            _dirtyRows[y] = true;
            _dirtyColumns[x] = true;
        }

        public void ClearDirty()
        {
            Array.Clear(_dirtyCells, 0, _dirtyCells.Length);
            Array.Clear(_dirtyRows, 0, _dirtyRows.Length);
            Array.Clear(_dirtyColumns, 0, _dirtyColumns.Length);
        }

        public void SwapChips(int x1, int y1, int x2, int y2)
        {
            if (x1 == x2 && y1 == y2)
                return;

            (_chips[x1, y1], _chips[x2, y2]) = (_chips[x2, y2], _chips[x1, y1]);

            MarkDirty(x1, y1);
            MarkDirty(x2, y2);
        }
    }
}