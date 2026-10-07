using System.Collections;
using TMPro;
using UnityEngine;

namespace Expedition33.Combat
{
    public class FloatingCombatText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _textMesh;
        [SerializeField] private float _floatSpeed = 1.2f;
        [SerializeField] private float _lifetime = 0.85f;

        public static void Spawn(Vector3 worldPosition, string message, Color color, float scaleMultiplier = 1f)
        {
            GameObject container = new GameObject("FloatingDamageText");
            container.transform.position = worldPosition + Vector3.up * 1.8f;

            FloatingCombatText popup = container.AddComponent<FloatingCombatText>();
            popup.Initialize(message, color, scaleMultiplier);
        }

        private void Initialize(string message, Color color, float scaleMultiplier)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(transform, false);

            _textMesh = textObj.AddComponent<TextMeshPro>();
            _textMesh.text = message;
            _textMesh.fontSize = 5f * scaleMultiplier;
            _textMesh.alignment = TextAlignmentOptions.Center;
            _textMesh.color = color;

            StartCoroutine(AnimateRoutine());
        }

        private IEnumerator AnimateRoutine()
        {
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Color startColor = _textMesh.color;

            // Face main camera
            Camera mainCam = Camera.main;

            while (elapsed < _lifetime)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _lifetime;

                transform.position = startPos + Vector3.up * (_floatSpeed * progress);

                if (mainCam != null)
                {
                    transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
                }

                // Fade out towards end
                if (progress > 0.5f)
                {
                    float alpha = Mathf.Lerp(1f, 0f, (progress - 0.5f) / 0.5f);
                    _textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
