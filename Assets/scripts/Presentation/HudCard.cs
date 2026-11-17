using UnityEngine;

public sealed class HudCard : MonoBehaviour
{
    [SerializeField] private GameObject frame;

    private void OnEnable()
    {
        if (frame != null)
            frame.SetActive(true);
    }

    private void OnDisable()
    {
        if (frame != null)
            frame.SetActive(false);
    }
}
