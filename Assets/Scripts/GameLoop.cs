using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Kdevaulo.Match3
{
    public class GameLoop
    {
        private readonly GridRefillHandler _gridRefillHandler;
        private readonly ChipController _chipController;
        private readonly InputBlocker _inputBlocker;
        private readonly GameEventBus _eventBus;
        private readonly MatchFinder _matchFinder;
        private readonly GridModel _gridModel;

        private readonly WaitForSeconds _delay;

        private Vector2Int _lastSwapFrom;
        private Vector2Int _lastSwapTo;

        private bool _hasPendingSwap;
        private bool _playerMoveRequested;

        public GameLoop(GridModel gridModel, MatchFinder matchFinder, ChipController chipController,
            GridRefillHandler gridRefillHandler, GameEventBus eventBus, InputBlocker inputBlocker)
        {
            _gridRefillHandler = gridRefillHandler;
            _chipController = chipController;
            _inputBlocker = inputBlocker;
            _matchFinder = matchFinder;
            _gridModel = gridModel;
            _eventBus = eventBus;

            _delay = new WaitForSeconds(0.5f);

            _eventBus.PlayerSwapPerformed += OnPlayerSwapPerformed;
        }

        public IEnumerator HandleGameLoop()
        {
            _inputBlocker.SetBlocked(true);

            _chipController.SpawnGrid();

            yield return HandleMatches(false);

            while (true)
            {
                if (_playerMoveRequested)
                {
                    _playerMoveRequested = false;
                    yield return HandleMatches(true);
                }

                yield return null;
            }
        }

        private IEnumerator PlayInvalidSwapFeedback()
        {
            var cells = new List<Vector2Int>(2) { _lastSwapFrom, _lastSwapTo };

            _chipController.HighlightCells(cells, Color.red);
            yield return _delay;

            _chipController.SwapWithoutNotify(_lastSwapFrom, _lastSwapTo);

            _chipController.ResetCellColors(cells);
            yield return _delay;
        }

        private void OnPlayerSwapPerformed(PlayerSwapPerformedEvent evt)
        {
            _playerMoveRequested = true;
            _hasPendingSwap = true;

            _lastSwapFrom = evt.From;
            _lastSwapTo = evt.To;
        }

        private IEnumerator HandleMatches(bool checkDirtyOnly)
        {
            _inputBlocker.SetBlocked(true);

            while (true)
            {
                var matches = _matchFinder.FindMatches(checkDirtyOnly);

                _gridModel.ClearDirty();

                if (matches.Count == 0)
                {
                    if (_hasPendingSwap)
                    {
                        yield return PlayInvalidSwapFeedback();
                        _hasPendingSwap = false;
                    }

                    break;
                }

                _hasPendingSwap = false;

                yield return _delay;

                foreach (var cluster in matches)
                {
                    _chipController.HighlightCells(cluster.Cells, Color.green);
                }

                yield return _delay;

                foreach (var cluster in matches)
                {
                    _chipController.ClearCells(cluster.Cells);
                }

                yield return _delay;

                var moveParams = _gridRefillHandler.ApplyGravity();
                _chipController.MoveByGravity(moveParams);

                yield return _delay;

                var spawns = _gridRefillHandler.SpawnMissingChips();
                _chipController.SpawnMissingChipsAboveGrid(spawns);

                yield return _delay;

                _chipController.MoveSpawnedChipsDown();
            }

            _inputBlocker.SetBlocked(false);
        }
    }
}