using UnityEngine;

namespace Kdevaulo.Match3
{
    [CreateAssetMenu(menuName = nameof(Match3) + "/" + nameof(GridSettings), fileName = nameof(GridSettings))]
    public class GridSettings : ScriptableObject
    {
        [field: SerializeField] public float CellSize { get; private set; }
        [field: SerializeField] public Vector2Int GridSize { get; private set; }
    }
}