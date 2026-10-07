using System;
using UnityEngine;

namespace Expedition33.Combat
{
    public enum QTEOutcome
    {
        None,
        Perfect,
        Good,
        Miss
    }

    [Serializable]
    public class QTEWindowDefinition
    {
        [SerializeField] private float _duration = 0.85f;
        [SerializeField] private float _sweetSpotTime = 0.60f;
        [SerializeField] private float _perfectHalfWindow = 0.08f;
        [SerializeField] private float _goodHalfWindow = 0.16f;

        public float Duration => _duration;
        public float SweetSpotTime => _sweetSpotTime;
        public float PerfectHalfWindow => _perfectHalfWindow;
        public float GoodHalfWindow => _goodHalfWindow;

        public QTEWindowDefinition()
        {
        }

        public QTEWindowDefinition(float duration, float sweetSpotTime, float perfectHalfWindow, float goodHalfWindow)
        {
            _duration = duration;
            _sweetSpotTime = sweetSpotTime;
            _perfectHalfWindow = perfectHalfWindow;
            _goodHalfWindow = goodHalfWindow;
        }
    }

    public static class QTEEvaluator
    {
        public static QTEOutcome Evaluate(float inputTime, QTEWindowDefinition window)
        {
            if (inputTime < 0f)
                return QTEOutcome.Miss;

            float diff = Mathf.Abs(inputTime - window.SweetSpotTime);

            if (diff <= window.PerfectHalfWindow)
            {
                return QTEOutcome.Perfect;
            }

            if (diff <= window.GoodHalfWindow)
            {
                return QTEOutcome.Good;
            }

            return QTEOutcome.Miss;
        }
    }
}
