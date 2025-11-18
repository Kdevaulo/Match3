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

        private void Start()
        {
            _chipController = new ChipController(_chipsContainer, _gridSettings, _chipSettings);
            _chipController.SpawnGrid();
        }
    }
}