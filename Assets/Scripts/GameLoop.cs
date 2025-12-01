using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Kdevaulo.Match3
{
    public class GameLoop
    {
        private const int MaxReshuffleAttempts = 3;

        private readonly GridRefillHandler _gridRefillHandler;
        private readonly ChipController _chipController;
        private readonly InputBlocker _inputBlocker;
        private readonly MatchFinder _matchFinder;
        private readonly MoveFinder _moveFinder;
        private readonly GridModel _gridModel;

        private readonly WaitForSeconds _delay;

        private Vector2Int _lastSwapFrom;
        private Vector2Int _lastSwapTo;

        private bool _hasPendingSwap;
        private bool _playerMoveRequested;

        public GameLoop(ChipSettings chipSettings, GridSettings gridSettings, RectTransform chipsContainer)
        {
            var eventBus = new GameEventBus();
            _gridModel = new GridModel(gridSettings.GridSize);

            _gridRefillHandler = new GridRefillHandler(_gridModel, chipSettings);
            _inputBlocker = new InputBlocker();
            _matchFinder = new MatchFinder(_gridModel);
            _moveFinder = new MoveFinder(_gridModel, _matchFinder);

            _chipController = new ChipController(chipsContainer, gridSettings, chipSettings, _gridModel, eventBus,
                _inputBlocker);

            _delay = new WaitForSeconds(0.1f);

            eventBus.PlayerSwapPerformed += OnPlayerSwapPerformed;
        }

        public IEnumerator HandleGameLoop()
        {
            _inputBlocker.SetBlocked(true);

            _chipController.SpawnGrid();

            yield return HandleMatches(false);
            yield return EnsureBoardHasMoves();

            while (true)
            {
                if (_playerMoveRequested)
                {
                    _playerMoveRequested = false;

                    yield return HandleMatches(true);
                    yield return EnsureBoardHasMoves();
                }

                yield return null;
            }
        }

        private IEnumerator PlayInvalidSwapFeedback()
        {
            var cells = new List<Vector2Int>(2) { _lastSwapFrom, _lastSwapTo };

            _chipController.HighlightCells(cells, ChipColors.WrongColor);
            yield return _delay;

            _chipController.SwapWithoutNotify(_lastSwapFrom, _lastSwapTo);

            _chipController.HighlightCells(cells, ChipColors.OrdinaryColor);
            yield return _delay;
        }

        private IEnumerator EnsureBoardHasMoves()
        {
            if (_moveFinder.HasAnyMove())
            {
                yield break;
            }

            _inputBlocker.SetBlocked(true);

            _chipController.ColorAllCells(ChipColors.WrongColor);
            yield return _delay;

            var attempts = 0;

            while (!_moveFinder.HasAnyMove())
            {
                _chipController.ReshuffleGrid();
                _chipController.ColorAllCells(ChipColors.OrdinaryColor);

                attempts++;

                if (attempts > MaxReshuffleAttempts)
                {
                    Debug.LogError("MaxReshuffleAttempts limit reached");
                    break;
                }

                yield return HandleMatches(false);
            }

            _inputBlocker.SetBlocked(false);
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
                    _chipController.HighlightCells(cluster.Cells, ChipColors.CorrectColor);
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