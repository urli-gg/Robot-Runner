using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public sealed class ToggleIndicator : MonoBehaviour
{
    [SerializeField] private TMP_Text state;
    [SerializeField] private Image knob;
    private Toggle toggle;

    private void Awake() => toggle = GetComponent<Toggle>();

    private void OnEnable()
    {
        toggle.onValueChanged.AddListener(Refresh);
        Refresh(toggle.isOn);
    }

    private void OnDisable() => toggle.onValueChanged.RemoveListener(Refresh);

    private void Refresh(bool enabled)
    {
        state.text = enabled ? "ON" : "OFF";
        state.color = enabled ? new Color32(198, 250, 255, 255) : new Color32(145, 164, 185, 255);
        knob.color = enabled ? new Color32(71, 239, 243, 255) : new Color32(104, 123, 149, 255);
        float x = enabled ? 0.78f : 0.20f;
        RectTransform handle = knob.rectTransform;
        handle.anchorMin = handle.anchorMax = new Vector2(x, 0.5f);
        state.rectTransform.anchorMin = new Vector2(enabled ? 0.08f : 0.37f, 0f);
        state.rectTransform.anchorMax = new Vector2(enabled ? 0.62f : 0.94f, 1f);
    }
}
