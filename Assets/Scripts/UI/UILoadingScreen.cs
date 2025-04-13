using Services;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class UILoadingScreen : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup _canvasGroup;
    [SerializeField]
    private Slider _loadingProgressSlider;

    public void SetAlpha(float newAlpha)
    {
        Assert.IsNotNull(_canvasGroup);
        _canvasGroup.alpha = newAlpha;
    }

    public void SetLoadingProgress(float progressPercentage)
    {
        Assert.IsNotNull(_loadingProgressSlider);
        _loadingProgressSlider.value = progressPercentage;
    }
}
