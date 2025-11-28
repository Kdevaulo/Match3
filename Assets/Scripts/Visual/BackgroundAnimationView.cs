using System;

using UnityEngine;

namespace Kdevaulo.Match3
{
    [Serializable]
    public struct IndexedTransform
    {
        public string Key;
        public RectTransform Transform;
    }

    public class BackgroundAnimationView : MonoBehaviour
    {
        [field: SerializeField] public IndexedTransform[] HorizontalTargets { get; private set; }
        [field: SerializeField] public IndexedTransform[] RotationTargets { get; private set; }
        [field: SerializeField] public IndexedTransform[] ScaleTargets { get; private set; }
    }
}