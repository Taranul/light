using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using System;

public class MenuButtonsAnimator : MonoBehaviour
{
    [SerializeField] float slideOffsetX = -300f;
    [SerializeField] float scrollSpeed = 750f;
    [SerializeField] float staggerDelay = 0.08f;
    [SerializeField] Ease easeType = Ease.OutCubic;

    float DurationPerButton => Mathf.Abs(slideOffsetX) / Mathf.Max(1f, scrollSpeed);

    [SerializeField] bool playInOnEnable = true;

    List<RectTransform> buttons = new();
    Dictionary<RectTransform, Vector2> homePositions = new();

    RectTransform containerRect;
    bool layoutFrozen;
    Sequence activeSequence;
    Coroutine pendingPlayIn;

    bool nextEntryIsForward = true;

    void Awake()
    {
        containerRect = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        if (playInOnEnable)
        {
            pendingPlayIn = StartCoroutine(PlayInNextFrame());
        }
    }

    void OnDisable()
    {
        if (pendingPlayIn != null)
        {
            StopCoroutine(pendingPlayIn);
            pendingPlayIn = null;
        }
        activeSequence?.Kill();
        foreach (var btn in buttons)
        {
            if (btn == null) continue;
            btn.DOKill();
            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg != null) cg.DOKill();
        }
    }

    public void SetNextDirection(bool isForward)
    {
        nextEntryIsForward = isForward;
    }

    IEnumerator PlayInNextFrame()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        PlayIn(nextEntryIsForward);
    }

    void CollectButtonsAndFreezeLayout()
    {
        buttons.Clear();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);

        foreach (RectTransform child in GetComponentsInChildren<RectTransform>(true))
        {
            if (child == containerRect) continue;
            if (child.parent != containerRect) continue;
            buttons.Add(child);
        }

        Debug.Log($"[{gameObject.name}] MenuButtonsAnimator: найдено кнопок = {buttons.Count}, containerRect.sizeDelta = {containerRect.sizeDelta}");

        if (!layoutFrozen)
        {
            Vector2 frozenSize = containerRect.sizeDelta;

            VerticalLayoutGroup layout = GetComponent<VerticalLayoutGroup>();
            if (layout != null) layout.enabled = false;

            ContentSizeFitter fitter = GetComponent<ContentSizeFitter>();
            if (fitter != null) fitter.enabled = false;

            if (frozenSize.x > 0.01f || frozenSize.y > 0.01f)
            {
                containerRect.sizeDelta = frozenSize;
                layoutFrozen = true;

                foreach (var btn in buttons)
                {
                    if (!homePositions.ContainsKey(btn))
                        homePositions[btn] = btn.anchoredPosition;
                }
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] MenuButtonsAnimator: посчитанный размер контейнера равен 0, не замораживаю его в этот раз.");
                if (layout != null) layout.enabled = true;
                if (fitter != null) fitter.enabled = true;
            }
        }
        else
        {
            foreach (var btn in buttons)
            {
                if (!homePositions.ContainsKey(btn))
                    homePositions[btn] = btn.anchoredPosition;
            }
        }
    }

    Vector2 GetHomePos(RectTransform btn)
    {
        if (homePositions.TryGetValue(btn, out var pos))
            return pos;

        return btn.anchoredPosition;
    }

    public void PlayIn()
    {
        PlayIn(nextEntryIsForward);
    }

    public void PlayIn(bool isForward)
    {
        Debug.Log($"[{gameObject.name}] MenuButtonsAnimator.PlayIn() вызван, isForward={isForward}");
        activeSequence?.Kill();
        CollectButtonsAndFreezeLayout();

        float dir = isForward ? 1f : -1f;
        float offset = Mathf.Abs(slideOffsetX) * dir;

        for (int i = 0; i < buttons.Count; i++)
        {
            RectTransform btn = buttons[i];
            btn.DOKill();

            Vector2 finalPos = GetHomePos(btn);
            Vector2 startPos = finalPos + new Vector2(offset, 0);

            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
            cg.DOKill();

            btn.anchoredPosition = startPos;
            cg.alpha = 0f;

            float delay = i * staggerDelay;

            btn.DOAnchorPos(finalPos, DurationPerButton).SetEase(easeType).SetDelay(delay);
            cg.DOFade(1f, DurationPerButton).SetDelay(delay);
        }
    }

    public void PlayOut(Action onComplete = null)
    {
        PlayOut(onComplete, true);
    }

    public void PlayOut(Action onComplete, bool isForward)
    {
        Debug.Log($"[{gameObject.name}] MenuButtonsAnimator.PlayOut() вызван, isForward={isForward}");

        if (buttons.Count == 0) CollectButtonsAndFreezeLayout();

        if (buttons.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] MenuButtonsAnimator: кнопок не найдено, сразу вызываю onComplete");
            onComplete?.Invoke();
            return;
        }

        activeSequence?.Kill();
        Sequence seq = DOTween.Sequence();

        float dir = isForward ? -1f : 1f;
        float offset = Mathf.Abs(slideOffsetX) * dir;

        for (int i = 0; i < buttons.Count; i++)
        {
            RectTransform btn = buttons[i];
            btn.DOKill();

            Vector2 startPos = GetHomePos(btn);
            Vector2 targetPos = startPos + new Vector2(offset, 0);

            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
            cg.DOKill();

            float delay = (buttons.Count - 1 - i) * staggerDelay;

            Tween moveTween = btn.DOAnchorPos(targetPos, DurationPerButton).SetEase(easeType);
            Tween fadeTween = cg.DOFade(0f, DurationPerButton);

            seq.Insert(delay, moveTween);
            seq.Insert(delay, fadeTween);
        }

        seq.OnComplete(() =>
        {
            Debug.Log($"[{gameObject.name}] MenuButtonsAnimator: PlayOut завершён, вызываю onComplete");
            onComplete?.Invoke();
        });

        activeSequence = seq;
    }
}