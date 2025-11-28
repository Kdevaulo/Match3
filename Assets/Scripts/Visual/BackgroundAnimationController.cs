using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Kdevaulo.Match3
{
    public class BackgroundAnimationController
    {
        private readonly BackgroundAnimationSettings _settings;
        private readonly BackgroundAnimationView _view;

        private readonly Dictionary<RectTransform, HorizontalMoveSettings> _horizontalAnimations =
            new Dictionary<RectTransform, HorizontalMoveSettings>();

        private readonly Dictionary<RectTransform, RotationAnimationSettings> _rotationAnimations =
            new Dictionary<RectTransform, RotationAnimationSettings>();

        private readonly Dictionary<RectTransform, ScaleAnimationSettings> _scaleAnimations =
            new Dictionary<RectTransform, ScaleAnimationSettings>();

        public BackgroundAnimationController(BackgroundAnimationSettings settings, BackgroundAnimationView view)
        {
            _settings = settings;
            _view = view;

            Prepare();
        }

        public IEnumerator Animate()
        {
            while (true)
            {
                yield return null;

                yield return CoroutineRunner.Parallel(
                    AnimateHorizontalAll(),
                    AnimateScaleAll(),
                    AnimateRotationAll()
                );
            }
        }

        private void Prepare()
        {
            BindAnimations(_view.HorizontalTargets, _settings.HorizontalMoves,
                _horizontalAnimations, cfg => cfg.Key);

            BindAnimations(_view.ScaleTargets, _settings.ScaleAnimations,
                _scaleAnimations, cfg => cfg.Key);

            BindAnimations(_view.RotationTargets, _settings.RotationAnimations,
                _rotationAnimations, cfg => cfg.Key);
        }

        private void BindAnimations<TSettings>(IndexedTransform[] targets, TSettings[] configs,
            Dictionary<RectTransform, TSettings> output, Func<TSettings, string> getKey)
        {
            foreach (var t in targets)
            {
                if (string.IsNullOrEmpty(t.Key) || t.Transform == null)
                    continue;

                foreach (var cfg in configs)
                {
                    if (getKey(cfg) == t.Key)
                    {
                        output[t.Transform] = cfg;
                        break;
                    }
                }
            }
        }

        private IEnumerator AnimateHorizontalAll()
        {
            while (true)
            {
                foreach (var kv in _horizontalAnimations)
                {
                    var transform = kv.Key;
                    var settings = kv.Value;

                    var time = Time.time * settings.Speed;
                    var t = (Mathf.Sin(time) + 1f) * 0.5f;
                    var result = Mathf.Lerp(settings.PositionRange.x, settings.PositionRange.y, t);

                    var position = transform.anchoredPosition;
                    position.x = result;
                    position.y = settings.PositionY;
                    transform.anchoredPosition = position;
                }

                yield return null;
            }
        }

        private IEnumerator AnimateScaleAll()
        {
            while (true)
            {
                foreach (var kv in _scaleAnimations)
                {
                    var transform = kv.Key;
                    var settings = kv.Value;

                    var time = Time.time * settings.Speed;
                    var t = (Mathf.Sin(time) + 1f) * 0.5f;
                    var result = Mathf.Lerp(settings.ScaleRange.x, settings.ScaleRange.y, t);

                    transform.localScale = new Vector3(result, result, 1f);
                }

                yield return null;
            }
        }

        private IEnumerator AnimateRotationAll()
        {
            while (true)
            {
                foreach (var kv in _rotationAnimations)
                {
                    var transform = kv.Key;
                    var settings = kv.Value;

                    var delta = settings.Speed * Time.deltaTime;
                    transform.Rotate(Vector3.forward * delta);
                }

                yield return null;
            }
        }
    }

    public static class CoroutineRunner
    {
        public static IEnumerator Parallel(params IEnumerator[] coroutines)
        {
            var routines = new List<IEnumerator>(coroutines);

            while (true)
            {
                for (var i = routines.Count - 1; i >= 0; i--)
                {
                    if (!routines[i].MoveNext())
                    {
                        routines.RemoveAt(i);
                    }
                }

                yield return null;
            }
        }
    }
}