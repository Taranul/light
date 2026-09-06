using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class EyesBlinkerController : MonoBehaviour
{
    [Header("Перетащи сюда все объекты-глаза")]
    List<RectTransform> eyes = new();
    [Header("Настройки моргания")]
    [SerializeField] float blinkDuration = 0.12f;
    [SerializeField] float minDelayBetween = 0.5f;
    [SerializeField] float maxDelayBetween = 3f;

    void Initalize(){
        
        foreach ( RectTransform eye in GetComponentsInChildren<RectTransform>() )
        {
            if (eye.GetComponent<EyesBlinkerController>() != null ) continue;
            eyes.Add(eye);

        }
    }

    void Start()
    {
        Initalize();

        foreach (var eye in eyes)
        {
            ScheduleNextBlink(eye);
        }
    }

    void ScheduleNextBlink(RectTransform eye)
    {
        float delay = Random.Range(minDelayBetween, maxDelayBetween);

        Sequence blink = DOTween.Sequence();
        blink.AppendInterval(delay);
        blink.Append(eye.DOScaleY(0.05f, blinkDuration).SetEase(Ease.InQuad));
        blink.Append(eye.DOScaleY(1f, blinkDuration).SetEase(Ease.OutQuad));
        blink.OnComplete(() => ScheduleNextBlink(eye));
    }

}
