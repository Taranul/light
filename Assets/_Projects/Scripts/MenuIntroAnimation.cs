using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class MenuIntroAnimation : MonoBehaviour
{
    [SerializeField] float slideOffsetX = -300f;
    [SerializeField] float durationPerButton = 0.4f;
    [SerializeField] float staggerDelay = 0.08f;
    [SerializeField] Ease easeType = Ease.OutCubic;

    List<RectTransform> buttons = new();

    void Initalize()
    {
        RectTransform containerRect = GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);

        foreach (RectTransform child in GetComponentsInChildren<RectTransform>())
        {
            if (child == transform) continue;
            if (child.parent != transform) continue;
            buttons.Add(child);
        }

        Vector2 frozenSize = containerRect.sizeDelta;

        VerticalLayoutGroup layout = GetComponent<VerticalLayoutGroup>();
        if (layout != null) layout.enabled = false;

        ContentSizeFitter fitter = GetComponent<ContentSizeFitter>();
        if (fitter != null) fitter.enabled = false;

        containerRect.sizeDelta = frozenSize;
    }

    void Start()
    {
        Initalize();
        PlayIntro();
    }

    public void PlayIntro()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            RectTransform btn = buttons[i];

            Vector2 finalPos = btn.anchoredPosition;
            Vector2 startPos = finalPos + new Vector2(slideOffsetX, 0);

            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();

            btn.anchoredPosition = startPos;
            cg.alpha = 0f;

            float delay = i * staggerDelay;

            btn.DOAnchorPos(finalPos, durationPerButton)
                .SetEase(easeType)
                .SetDelay(delay);

            cg.DOFade(1f, durationPerButton)
                .SetDelay(delay);
        }
    }
}