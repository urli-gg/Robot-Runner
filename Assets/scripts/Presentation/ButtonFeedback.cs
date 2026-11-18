using Dino.Utility.Audio;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class ButtonFeedback : MonoBehaviour
{
    private Button button;

    private void Awake() => button = GetComponent<Button>();
    private void OnEnable() => button.onClick.AddListener(Click);
    private void OnDisable() => button.onClick.RemoveListener(Click);
    private static void Click() => AudioManager.Instance?.PlaySound("click");
}
