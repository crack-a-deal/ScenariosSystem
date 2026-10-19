using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectionIndicator : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Image _progress;
    [SerializeField] private TMP_Text _label;

    private void Awake()
    {
        Hide();
    }

    public void Show(string label)
    {
        _root.SetActive(true);
        _progress.fillAmount = 0f;
        _label.text = label;
    }

    public void SetProgress(float progress)
    {
        _progress.fillAmount = Mathf.Clamp01(progress);
    }

    public void Hide()
    {
        _root.SetActive(false);
        _progress.fillAmount = 0f;
    }
}
