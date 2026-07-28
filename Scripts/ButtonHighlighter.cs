using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gamebee
{
    public class ButtonHighlighter : MonoBehaviour
    {
        public GameObject defaultButton;
        private RectTransform Bracket => GetOrCreateBracket();
        private RectTransform _bracket;

        private Canvas _canvas;
        private RectTransform currentSelected;
        private float scaleOffset = 1;
        private float scaleMin = 1, scaleMax = 1.2f;
        private float alpha = 0, alphaStep = 2f;
        private Vector3 currentPosition;

        private void Start()
        {
            enabled = GBPManager.IsTV();
            Bracket.gameObject.SetActive(enabled);
        }

        private RectTransform GetOrCreateBracket()
        {
            if (_bracket) return _bracket;

            var bracketPrefab = Resources.Load<GameObject>("bracket");
            _bracket = Instantiate(bracketPrefab, transform).GetComponent<RectTransform>();
            _bracket.SetParent(_canvas ? _canvas.transform : transform);
            _bracket.GetComponent<Image>().raycastTarget = false;

            return _bracket;
        }

        private void Update()
        {
            var delta = Time.unscaledDeltaTime;

            if (currentSelected && currentSelected.gameObject.activeSelf)
            {
                alpha += alphaStep * delta;
                var step = CircularEaseInOutCached(Mathf.PingPong(alpha, 1));
                scaleOffset = Mathf.Lerp(scaleMin, scaleMax, step);

                Bracket.localScale = Vector3.one * scaleOffset;

                currentPosition = currentSelected.TransformPoint(currentSelected.rect.center);
                Bracket.position = Vector3.MoveTowards(Bracket.position, currentPosition, 1000 * delta);
            }

            var go = EventSystem.current.currentSelectedGameObject;
            if (!go || go is not { activeInHierarchy: true }) go = defaultButton;
            if (!go || go is not { activeInHierarchy: true })
            {
                currentSelected = null;
                Bracket.gameObject.SetActive(false);
                return;
            }

            if (currentSelected && go.Equals(currentSelected.gameObject)) return;

            currentSelected = go.GetComponent<RectTransform>();
            _canvas = go.GetComponentInParent<Canvas>();

            if (!currentSelected) return;

            Bracket.gameObject.SetActive(true);
            Bracket.SetParent(_canvas.transform);
            Bracket.SetAsLastSibling();

            Bracket.pivot = new Vector2(0.5f, 0.5f);

            Vector2 size = currentSelected.sizeDelta;
            if (size.x <= 0 || size.y <= 0)
            {
                if (currentSelected.GetComponentInChildren<Image>() is { } elem)
                {
                    size.x = elem.preferredWidth;
                    size.y = elem.preferredHeight;
                }
            }

            size.x = Mathf.Max(size.x, 100);
            size.y = Mathf.Max(size.y, 100);

            Bracket.sizeDelta = size;
        }

        private static SortedDictionary<float, float> circularEaseInOutCache = new();

        public static float CircularEaseInOutCached(float p)
        {
            p = (float)Math.Round(p, 2);
            if (circularEaseInOutCache.TryGetValue(p, out var value))
                return value;

            value = CircularEaseInOut(p);
            circularEaseInOutCache.Add(p, value);
            return value;
        }

        /// <summary>	
        /// Modeled after the piecewise circular function
        /// y = (1/2)(1 - Math.Sqrt(1 - 4x^2))           ; [0, 0.5)
        /// y = (1/2)(Math.Sqrt(-(2x - 3)*(2x - 1)) + 1) ; [0.5, 1]
        /// </summary>
        private static float CircularEaseInOut(float p) =>
            p switch
            {
                < 0.5f => 0.5f * (1 - (float)Math.Sqrt(1 - 4 * (p * p))),
                _ => 0.5f * ((float)Math.Sqrt(-((2 * p) - 3) * ((2 * p) - 1)) + 1)
            };

        private void OnDestroy()
        {
            Destroy(_bracket?.gameObject);
        }
    }
}