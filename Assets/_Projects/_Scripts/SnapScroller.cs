using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SnapScroller : MonoBehaviour, IScrollHandler
{
    [SerializeField] private RectTransform _viewportRectTransform;
    [SerializeField] private RectTransform _contentRectTransform;

    private readonly List<RectTransform> _skins = new();
    private int _selectedSkinIndex;

    private void Awake()
    {
        InitializeSkins();
        ChangeSnappedObject(0);
    }

    public void ChangeSnappedObject(int index)
    {
        if (_skins.Count == 0)
            return;

        _selectedSkinIndex = Mathf.Clamp(index, 0, _skins.Count - 1);

        Vector3 viewportCenter = _viewportRectTransform.TransformPoint(
            _viewportRectTransform.rect.center);

        RectTransform selectedSkin = _skins[_selectedSkinIndex];

        Vector3 skinCenter = selectedSkin.TransformPoint(
            selectedSkin.rect.center);

        Vector3 offset = viewportCenter - skinCenter;

        _contentRectTransform.position += new Vector3(offset.x, 0f, 0f);
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (_skins.Count == 0 || eventData.scrollDelta.y == 0)
            return;

        int direction = (int)Mathf.Sign(eventData.scrollDelta.y);

        ChangeSnappedObject(_selectedSkinIndex + direction);
    }

    private void InitializeSkins()
    {
        foreach (Transform child in _contentRectTransform)
        {
            _skins.Add((RectTransform)child);
        }
    }
}