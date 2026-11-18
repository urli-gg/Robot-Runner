using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public sealed class SafeAreaRect : MonoBehaviour
{
    private RectTransform content;
    private Rect lastArea;
    private Vector2Int lastSize;

    private void OnEnable()
    {
        content = GetComponent<RectTransform>();
        lastSize = Vector2Int.zero;
        Apply();
    }

    private void Update() => Apply();

    private void Apply()
    {
        var size = new Vector2Int(Screen.width, Screen.height);
        Rect area = Screen.safeArea;
        if (size.x <= 0 || size.y <= 0 || (size == lastSize && area == lastArea))
            return;
        lastSize = size;
        lastArea = area;
        content.anchorMin = new Vector2(area.xMin / size.x, area.yMin / size.y);
        content.anchorMax = new Vector2(area.xMax / size.x, area.yMax / size.y);
        content.offsetMin = Vector2.zero;
        content.offsetMax = Vector2.zero;
    }
}
