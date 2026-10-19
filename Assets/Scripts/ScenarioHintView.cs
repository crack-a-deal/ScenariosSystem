using TMPro;
using UnityEngine;

public class ScenarioHintView : MonoBehaviour
{
    [SerializeField] private TMP_Text _hintText;
    [SerializeField] private GameObject block;
    [SerializeField] private ScenarioRunner _runner;

    private void Awake()
    {
        _runner.StepStarted += _runner_StepStarted;
        _runner.ScenarioCompleted += _runner_ScenarioCompleted;
    }

    private void _runner_ScenarioCompleted()
    {
        block.SetActive(false);
    }

    private void _runner_StepStarted(Step obj)
    {
        _hintText.SetText(obj.Hint);
    }
}