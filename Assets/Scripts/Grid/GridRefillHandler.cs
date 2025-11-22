using System.Collections.Generic;

namespace Kdevaulo.Match3
{
    public struct MoveParams
    {
        public int FromX { get; }
        public int FromY { get; }
        public int ToX { get; }
        public int ToY { get; }

        public MoveParams(int fromX, int fromY, int toX, int toY)
        {
            FromX = fromX;
            FromY = fromY;
            ToX = toX;
            ToY = toY;
        }
    }

    public struct SpawnParams
    {
        public int X { get; }
        public int Y { get; }
        public Chip Type { get; }
        public int OrderFromTop { get; }

        public SpawnParams(int x, int y, Chip type, int orderFromTop)
        {
            X = x;
            Y = y;
            Type = type;
            OrderFromTop = orderFromTop;
        }
    }

    public class GridRefillHandler
    {
        private readonly ChipSettings _chipSettings;
        private readonly GridModel _gridModel;

        public GridRefillHandler(GridModel gridModel, ChipSettings chipSettings)
        {
            _gridModel = gridModel;
            _chipSettings = chipSettings;
        }

        public List<MoveParams> ApplyGravity()
        {
            var moves = new List<MoveParams>();

            var width = _gridModel.Width;
            var height = _gridModel.Height;

            for (var x = 0; x < width; x++)
            {
                var targetY = 0;

                for (var y = 0; y < height; y++)
                {
                    var type = _gridModel.GetChip(x, y);

                    if (type == Chip.None)
                        continue;

                    if (y != targetY)
                    {
                        _gridModel.SetChip(x, targetY, type);
                        _gridModel.SetChip(x, y, Chip.None);

                        moves.Add(new MoveParams(x, y, x, targetY));
                    }

                    targetY++;
                }
            }

            return moves;
        }

        public List<SpawnParams> SpawnMissingChips()
        {
            var spawns = new List<SpawnParams>();

            var width = _gridModel.Width;
            var height = _gridModel.Height;

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
                    var type = _chipSettings.GetRandomType();

                    _gridModel.SetChip(x, y, type);

                    spawns.Add(new SpawnParams(x, y, type, i));
                }
            }

            return spawns;
        }
    }
}