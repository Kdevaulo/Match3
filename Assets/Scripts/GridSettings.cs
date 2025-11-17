using UnityEngine;

namespace Kdevaulo.Match3
{
    [CreateAssetMenu(menuName = nameof(Match3) + "/" + nameof(GridSettings), fileName = nameof(GridSettings))]
    public class GridSettings : ScriptableObject
    {
        [field: Header("Values")]
        [field: SerializeField] public float CellSize { get; private set; }

        [field: Header("References")]
        [field: SerializeField] public Vector2Int GridSize { get; private set; }
        [field: SerializeField] public Sprite[] ChipSprites { get; private set; }
        [field: SerializeField] public ChipView ChipViewPrefab { get; private set; }

        public Sprite GetRandomSprite()
        {
            if (ChipSprites == null || ChipSprites.Length == 0)
                return null;

            int index = Random.Range(0, ChipSprites.Length);
            return ChipSprites[index];
        }
    }
}