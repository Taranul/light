using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Expedition33.Combat
{
    /// <summary>
    /// Displays the upcoming turn order as a vertical column of round actor icons
    /// in the top-left HUD, shifting the active actor's icon to the right with a
    /// highlight halo during its turn and returning it to the column on turn end.
    /// </summary>
    public class TurnTimelineUI : MonoBehaviour
    {
        private class SlotView
        {
            public RectTransform Root;
            public CanvasGroup CanvasGroup;
            public Image HighlightRing;
            public Image BorderImage;
            public Image IconImage;
        }

        [Header("Layout")]
        [SerializeField] private int _maxVisibleSlots = 6;
        [SerializeField] private float _iconSize = 54f;
        [SerializeField] private float _slotSpacing = 68f;
        [SerializeField] private float _topPadding = 34f;
        [SerializeField] private float _activeShiftX = 28f;
        [SerializeField] private float _activeScale = 1.18f;

        [Header("Colors")]
        [SerializeField] private Color _playerIconColor = new Color(0.20f, 0.68f, 1.00f, 1f);
        [SerializeField] private Color _enemyIconColor = new Color(0.95f, 0.25f, 0.34f, 1f);
        [SerializeField] private Color _playerHighlightColor = new Color(0.45f, 0.90f, 1.00f, 1f);
        [SerializeField] private Color _enemyHighlightColor = new Color(1.00f, 0.55f, 0.30f, 1f);

        private readonly List<SlotView> _slots = new();
        private Sequence _transitionSequence;
        private Tweener _ringPulseTween;
        private bool _isActiveTurnExtended;

        private Texture2D _circleTex;
        private Texture2D _ringTex;
        private Texture2D _placeholderTex;
        private Sprite _circleSprite;
        private Sprite _ringSprite;
        private Sprite _placeholderSprite;

        private void Awake()
        {
            EnsureSprites();
            BuildSlotPool();
        }

        private void OnDestroy()
        {
            KillTweens();
            DestroyGeneratedSprite(_circleSprite, _circleTex);
            DestroyGeneratedSprite(_ringSprite, _ringTex);
            DestroyGeneratedSprite(_placeholderSprite, _placeholderTex);
        }

        /// <summary>
        /// Immediately populates the vertical column without shifting any slot right.
        /// </summary>
        public void SetTimeline(IReadOnlyList<CombatActorStats> upcoming)
        {
            if (_slots.Count == 0)
                BuildSlotPool();

            KillTweens();
            _isActiveTurnExtended = false;

            ApplyUpcomingToSlots(upcoming);
            ResetSlotsToColumn();
        }

        /// <summary>
        /// Shifts the top (active) actor's round icon to the right and enables its highlight halo.
        /// </summary>
        public void ActivateCurrentTurn(float duration = 0.22f)
        {
            if (_slots.Count == 0)
                return;

            KillTweens();
            _isActiveTurnExtended = true;

            SlotView activeSlot = _slots[0];
            if (!activeSlot.Root.gameObject.activeSelf)
                return;

            TweenAnchorPos(activeSlot.Root, new Vector2(_activeShiftX, GetSlotY(0)), duration).SetEase(Ease.OutBack);
            activeSlot.Root.DOScale(_activeScale, duration).SetEase(Ease.OutBack);
            ActivateHighlightRing(activeSlot, duration);
        }

        /// <summary>
        /// Returns the active actor's icon back to its aligned position in the column.
        /// </summary>
        public void ReturnActiveIconToColumn(float duration = 0.14f)
        {
            if (_slots.Count == 0 || !_isActiveTurnExtended)
                return;

            KillTweens();
            _isActiveTurnExtended = false;

            SlotView activeSlot = _slots[0];
            TweenAnchorPos(activeSlot.Root, new Vector2(0f, GetSlotY(0)), duration).SetEase(Ease.InOutQuad);
            activeSlot.Root.DOScale(1f, duration).SetEase(Ease.InOutQuad);
            if (activeSlot.HighlightRing != null)
            {
                TweenFade(activeSlot.HighlightRing, 0f, duration);
            }
        }

        /// <summary>
        /// Animates the finishing character's icon returning to the column, slides the
        /// remaining queue upward, and shifts the next active character's icon to the right.
        /// </summary>
        public void TransitionToNextTurn(IReadOnlyList<CombatActorStats> nextUpcoming, float totalDuration = 0.36f)
        {
            if (_slots.Count == 0)
                BuildSlotPool();

            KillTweens();

            float returnDur = totalDuration * 0.34f;
            float slideDur = totalDuration * 0.34f;
            float activateDur = totalDuration * 0.32f;

            SlotView finishingSlot = _slots[0];
            _transitionSequence = DOTween.Sequence();

            // 1. Return active icon back into its column position
            if (_isActiveTurnExtended && finishingSlot.Root.gameObject.activeSelf)
            {
                _transitionSequence.Append(TweenAnchorPosX(finishingSlot.Root, 0f, returnDur).SetEase(Ease.InOutQuad));
                _transitionSequence.Join(finishingSlot.Root.DOScale(1f, returnDur).SetEase(Ease.InOutQuad));
                if (finishingSlot.HighlightRing != null)
                {
                    _transitionSequence.Join(TweenFade(finishingSlot.HighlightRing, 0f, returnDur));
                }
            }

            // 2. Fade finishing icon out at top while remaining slots slide up one step
            _transitionSequence.AppendCallback(() =>
            {
                if (finishingSlot.Root.gameObject.activeSelf)
                {
                    TweenFade(finishingSlot.CanvasGroup, 0f, slideDur).SetEase(Ease.OutQuad);
                    TweenAnchorPosY(finishingSlot.Root, GetSlotY(0) + _slotSpacing * 0.4f, slideDur).SetEase(Ease.OutQuad);
                }

                for (int i = 1; i < _slots.Count; i++)
                {
                    if (_slots[i].Root.gameObject.activeSelf)
                    {
                        TweenAnchorPosY(_slots[i].Root, GetSlotY(i - 1), slideDur).SetEase(Ease.OutCubic);
                    }
                }
            });

            _transitionSequence.AppendInterval(slideDur);

            // 3. Rotate slot list, refresh with updated queue, and step the new active icon right
            _transitionSequence.AppendCallback(() =>
            {
                _slots.RemoveAt(0);
                _slots.Add(finishingSlot);

                ApplyUpcomingToSlots(nextUpcoming);
                ResetSlotsToColumn();

                int lastVisible = Mathf.Min(_slots.Count, nextUpcoming != null ? nextUpcoming.Count : 0) - 1;
                if (lastVisible > 0)
                {
                    SlotView bottomSlot = _slots[lastVisible];
                    float targetAlpha = bottomSlot.CanvasGroup.alpha;
                    bottomSlot.CanvasGroup.alpha = 0f;
                    TweenFade(bottomSlot.CanvasGroup, targetAlpha, activateDur).SetEase(Ease.OutQuad);
                }

                _isActiveTurnExtended = true;
                SlotView nextActive = _slots[0];
                if (nextActive.Root.gameObject.activeSelf)
                {
                    TweenAnchorPosX(nextActive.Root, _activeShiftX, activateDur).SetEase(Ease.OutBack);
                    nextActive.Root.DOScale(_activeScale, activateDur).SetEase(Ease.OutBack);
                    ActivateHighlightRing(nextActive, activateDur);
                }
            });
        }

        private void BuildSlotPool()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            _slots.Clear();

            for (int i = 0; i < _maxVisibleSlots; i++)
            {
                _slots.Add(CreateSlot($"TurnSlot_{i}", i));
            }
        }

        private SlotView CreateSlot(string slotName, int index)
        {
            var rootObj = new GameObject(slotName);
            rootObj.transform.SetParent(transform, false);

            var rootRect = rootObj.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 1f);
            rootRect.anchorMax = new Vector2(0.5f, 1f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(_iconSize, _iconSize);
            rootRect.anchoredPosition = new Vector2(0f, GetSlotY(index));

            var cg = rootObj.AddComponent<CanvasGroup>();

            // Outer highlight halo ring
            var ringObj = new GameObject("HighlightRing");
            ringObj.transform.SetParent(rootObj.transform, false);
            var ringRect = ringObj.AddComponent<RectTransform>();
            ringRect.anchorMin = Vector2.zero;
            ringRect.anchorMax = Vector2.one;
            ringRect.offsetMin = new Vector2(-8f, -8f);
            ringRect.offsetMax = new Vector2(8f, 8f);
            var ringImg = ringObj.AddComponent<Image>();
            ringImg.sprite = _ringSprite;
            ringImg.color = new Color(1f, 1f, 1f, 0f);
            ringImg.raycastTarget = false;

            // Round border + circular mask for portraits
            var borderObj = new GameObject("CircleFrame");
            borderObj.transform.SetParent(rootObj.transform, false);
            var borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
            var borderImg = borderObj.AddComponent<Image>();
            borderImg.sprite = _circleSprite;
            borderImg.color = new Color(0.10f, 0.14f, 0.20f, 0.95f);
            borderImg.raycastTarget = false;
            var mask = borderObj.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            // Inner round portrait / placeholder icon
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(borderObj.transform, false);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(4f, 4f);
            iconRect.offsetMax = new Vector2(-4f, -4f);
            var iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = _placeholderSprite;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            return new SlotView
            {
                Root = rootRect,
                CanvasGroup = cg,
                HighlightRing = ringImg,
                BorderImage = borderImg,
                IconImage = iconImg
            };
        }

        private void ApplyUpcomingToSlots(IReadOnlyList<CombatActorStats> upcoming)
        {
            int count = upcoming != null ? upcoming.Count : 0;

            for (int i = 0; i < _slots.Count; i++)
            {
                SlotView slot = _slots[i];
                if (i >= count)
                {
                    slot.Root.gameObject.SetActive(false);
                    continue;
                }

                CombatActorStats actor = upcoming[i];
                slot.Root.gameObject.SetActive(true);

                bool hasCustomPortrait = actor.PortraitIcon != null;
                slot.IconImage.sprite = hasCustomPortrait ? actor.PortraitIcon : _placeholderSprite;
                slot.IconImage.color = hasCustomPortrait
                    ? Color.white
                    : (actor.IsPlayer ? _playerIconColor : _enemyIconColor);

                slot.BorderImage.color = actor.IsPlayer
                    ? new Color(0.08f, 0.18f, 0.30f, 0.96f)
                    : new Color(0.28f, 0.08f, 0.12f, 0.96f);

                if (slot.HighlightRing != null)
                {
                    Color ringColor = actor.IsPlayer ? _playerHighlightColor : _enemyHighlightColor;
                    ringColor.a = 0f;
                    slot.HighlightRing.color = ringColor;
                }
            }
        }

        private void ResetSlotsToColumn()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                SlotView slot = _slots[i];
                slot.Root.DOKill();
                slot.CanvasGroup.DOKill();
                slot.Root.anchoredPosition = new Vector2(0f, GetSlotY(i));
                slot.Root.localScale = Vector3.one;
                slot.CanvasGroup.alpha = i == 0 ? 1f : Mathf.Lerp(0.88f, 0.62f, (float)i / Mathf.Max(1, _slots.Count - 1));

                if (slot.HighlightRing != null)
                {
                    slot.HighlightRing.DOKill();
                    slot.HighlightRing.rectTransform.localScale = Vector3.one;
                    Color c = slot.HighlightRing.color;
                    c.a = 0f;
                    slot.HighlightRing.color = c;
                }
            }
        }

        private void ActivateHighlightRing(SlotView slot, float fadeDuration)
        {
            slot.CanvasGroup.alpha = 1f;
            if (slot.HighlightRing == null)
                return;

            slot.HighlightRing.DOKill();
            slot.HighlightRing.rectTransform.localScale = Vector3.one;
            TweenFade(slot.HighlightRing, 0.95f, fadeDuration);

            _ringPulseTween?.Kill();
            _ringPulseTween = slot.HighlightRing.rectTransform
                .DOScale(1.10f, 0.75f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private float GetSlotY(int index)
        {
            return -_topPadding - index * _slotSpacing;
        }

        private void KillTweens()
        {
            _transitionSequence?.Kill();
            _transitionSequence = null;
            _ringPulseTween?.Kill();
            _ringPulseTween = null;

            foreach (SlotView slot in _slots)
            {
                slot.Root?.DOKill();
                slot.CanvasGroup?.DOKill();
                if (slot.HighlightRing != null)
                {
                    slot.HighlightRing.DOKill();
                    slot.HighlightRing.rectTransform.DOKill();
                }
            }
        }

        private static Tweener TweenAnchorPos(RectTransform rect, Vector2 endValue, float duration)
        {
            return DOTween.To(() => rect.anchoredPosition, v => rect.anchoredPosition = v, endValue, duration)
                .SetTarget(rect);
        }

        private static Tweener TweenAnchorPosX(RectTransform rect, float endX, float duration)
        {
            return DOTween.To(
                () => rect.anchoredPosition.x,
                x => rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y),
                endX,
                duration
            ).SetTarget(rect);
        }

        private static Tweener TweenAnchorPosY(RectTransform rect, float endY, float duration)
        {
            return DOTween.To(
                () => rect.anchoredPosition.y,
                y => rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y),
                endY,
                duration
            ).SetTarget(rect);
        }

        private static Tweener TweenFade(CanvasGroup group, float endAlpha, float duration)
        {
            return DOTween.To(() => group.alpha, a => group.alpha = a, endAlpha, duration)
                .SetTarget(group);
        }

        private static Tweener TweenFade(Image image, float endAlpha, float duration)
        {
            return DOTween.To(
                () => image.color.a,
                a =>
                {
                    Color c = image.color;
                    c.a = a;
                    image.color = c;
                },
                endAlpha,
                duration
            ).SetTarget(image);
        }

        private void EnsureSprites()
        {
            if (_circleSprite != null)
                return;

            const int size = 64;
            float center = (size - 1) * 0.5f;

            _circleTex = CreateTexture(size);
            _ringTex = CreateTexture(size);
            _placeholderTex = CreateTexture(size);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));

                    // Solid smooth circle
                    float circleAlpha = Mathf.Clamp01(30.5f - dist);
                    _circleTex.SetPixel(x, y, new Color(1f, 1f, 1f, circleAlpha));

                    // Outer highlight halo ring
                    float outerEdge = Mathf.Clamp01(31.0f - dist);
                    float innerEdge = Mathf.Clamp01(dist - 24.5f);
                    float ringAlpha = Mathf.Min(outerEdge, innerEdge);
                    _ringTex.SetPixel(x, y, new Color(1f, 1f, 1f, ringAlpha));

                    // Placeholder round icon with subtle radial shading and inner rim
                    float innerCircleAlpha = Mathf.Clamp01(28.0f - dist);
                    float shade = Mathf.Lerp(1.0f, 0.72f, Mathf.Clamp01(dist / 28.0f));
                    bool isEmblemRim = dist >= 16.5f && dist <= 18.5f;
                    float brightness = isEmblemRim ? Mathf.Min(1f, shade + 0.22f) : shade;
                    _placeholderTex.SetPixel(x, y, new Color(brightness, brightness, brightness, innerCircleAlpha));
                }
            }

            _circleTex.Apply();
            _ringTex.Apply();
            _placeholderTex.Apply();

            var rect = new Rect(0, 0, size, size);
            var pivot = new Vector2(0.5f, 0.5f);
            _circleSprite = Sprite.Create(_circleTex, rect, pivot);
            _ringSprite = Sprite.Create(_ringTex, rect, pivot);
            _placeholderSprite = Sprite.Create(_placeholderTex, rect, pivot);
        }

        private static Texture2D CreateTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        private static void DestroyGeneratedSprite(Sprite sprite, Texture2D tex)
        {
            if (sprite != null) Destroy(sprite);
            if (tex != null) Destroy(tex);
        }
    }
}
