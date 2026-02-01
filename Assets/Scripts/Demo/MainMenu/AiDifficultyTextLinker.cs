using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AiDifficultyTextLinker : MonoBehaviour
{
    [SerializeField] private TMP_Text _linkedText;
    [SerializeField] private Slider _linkedSlider;

    private void Start()
    {
        _linkedSlider.onValueChanged.AddListener(HandleSliderValueChanged);
        HandleSliderValueChanged(_linkedSlider.value);

    }

    private void OnDestroy()
    {
        _linkedSlider.onValueChanged.RemoveListener(HandleSliderValueChanged);
    }

    private void HandleSliderValueChanged(float value)
    {
        _linkedText.text = $"AI difficulty level: {GetText(value)}";
    }

    private string GetText(float value)
    {
        switch (value)
        {
            case 0f:
            default:
                return "Easy";
            case 1f:
                return "Medium";
            case 2f:
                return "Hard";
        }
    }
}
