using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionIndicator : MonoBehaviour
{
    [SerializeField] private TMP_Text _promptText;
    [SerializeField] private Slider _progressBar;

    private void Awake()
    {
        Hide();
    }

    public void Show(string prompt)
    {
        gameObject.SetActive(true);
        _promptText.text = prompt;
        SetProgress(0f);
    }

    public void SetProgress(float progress)
    {
        _progressBar.value = progress;
    }

    public void Hide()
    {
        _progressBar.value = 0f;
        gameObject.SetActive(false);
    }
}