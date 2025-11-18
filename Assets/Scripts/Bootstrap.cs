using UnityEngine;

namespace Kdevaulo.Match3
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private RectTransform _chipsContainer;
        [SerializeField] private ChipSettings _chipSettings;
        [SerializeField] private GridSettings _gridSettings;

        private ChipView[,] _chips;

        private ChipController _chipController;
        private MatchFinder _matchFinder;
        private GridModel _gridModel;

        private void Start()
        {
            _gridModel = new GridModel(_gridSettings.GridSize);

            _chipController = new ChipController(_chipsContainer, _gridSettings, _chipSettings, _gridModel);
            _chipController.SpawnGrid();

            _matchFinder = new MatchFinder(_gridModel);

            var allMatches = _matchFinder.FindMatches(false);

            foreach (var cluster in allMatches)
            {
                Debug.Log($"Shape - {cluster.Shape}, Type - {cluster.Type}");
            }
        }
    }
}