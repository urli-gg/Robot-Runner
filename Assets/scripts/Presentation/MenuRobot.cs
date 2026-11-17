using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public sealed class MenuRobot : MonoBehaviour
{
    [SerializeField] private FactoryArt art;
    private Image portrait;
    private int previousFrame = -1;

    private void Awake() => portrait = GetComponent<Image>();

    private void Update()
    {
        int frame = (int)(Time.unscaledTime * 4.5f) % art.idle.Length;
        if (frame == previousFrame)
            return;
        previousFrame = frame;
        portrait.sprite = art.idle[frame];
    }
}
