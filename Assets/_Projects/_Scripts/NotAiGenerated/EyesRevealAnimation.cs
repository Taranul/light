using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class EyesRevealAnimation : MonoBehaviour
{
    [Header("Настройки появления")]
    [SerializeField] float fadeDuration = 0.5f;
    [SerializeField] float staggerDelay = 0.3f;
    [SerializeField] int groupSize = 1;
    [SerializeField] Ease easeType = Ease.OutQuad;

    List<Image> eyes = new();

    void Initalize()
    {
        foreach (Image eye in GetComponentsInChildren<Image>())
        {
            eyes.Add(eye);

            Color c = eye.color;
            c.a = 0f;
            eye.color = c;
        }
    }

    void Start()
    {
        Initalize();
        PlayReveal();
    }

    public void PlayReveal()
    {
        for (int i = 0; i < eyes.Count; i++)
        {
            Image eye = eyes[i];
            int groupIndex = i / groupSize;
            float delay = groupIndex * staggerDelay;

            eye.DOFade(1f, fadeDuration)
                .SetDelay(delay)
                .SetEase(easeType);
        }
    }
}