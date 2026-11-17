using DG.Tweening;
using UnityEngine;

public class UIWindow : MonoBehaviour
{
    [SerializeField] private string _id;
    [SerializeField] private RectTransform _canvasRectTransform;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private bool _hideOnStart;
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float hideDuration = 0.5f;
    [SerializeField] private Ease showEase = Ease.OutCubic;
    [SerializeField] private Ease hideEase = Ease.InCubic;

    private Tween fade;
    private bool initialized;

    public CanvasGroup CanvasGroup => _canvasGroup;
    public RectTransform CanvasRectTransform => _canvasRectTransform;
    public string Id => _id;

    private void Start()
    {
        if (!initialized)
            Initialize();
    }

    public virtual void Initialize()
    {
        initialized = true;
        if (_hideOnStart)
            Hide(true);
        else if (GameManager.Instance == null || !GameManager.Instance.HasStarted)
            Show();
    }

    public virtual void Show(bool instant = false)
    {
        fade?.Kill();
        bool wasVisible = _canvasRectTransform.gameObject.activeSelf && _canvasGroup.interactable;
        if (!wasVisible || !initialized || Id == "mainmenuui")
            _canvasGroup.alpha = instant ? 1f : 0f;
        _canvasRectTransform.gameObject.SetActive(true);
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        if (instant)
        {
            _canvasGroup.alpha = 1f;
            return;
        }

        fade = _canvasGroup.DOFade(1f, Mathf.Min(showDuration, 0.32f)).SetEase(showEase).SetUpdate(true);
    }

    public virtual void Hide(bool instant = false)
    {
        fade?.Kill();
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        if (instant)
        {
            _canvasGroup.alpha = 0f;
            _canvasRectTransform.gameObject.SetActive(false);
            return;
        }

        fade = _canvasGroup.DOFade(0f, Mathf.Min(hideDuration, 0.20f)).SetEase(hideEase).SetUpdate(true)
            .OnComplete(() => _canvasRectTransform.gameObject.SetActive(false));
    }

    private void OnDestroy() => fade?.Kill();
}
