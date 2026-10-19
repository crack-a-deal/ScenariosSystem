using TMPro;
using UnityEngine;

public class MatchingQuestionView : MonoBehaviour
{
    [SerializeField] private TMP_Text questionText;

    [SerializeField] private ReordableListView leftReordableList;
    [SerializeField] private ReordableListView rightReordableList;

    public ReordableListView LeftReordableList => leftReordableList;
    public ReordableListView RightReordableList => rightReordableList;

    public void SetQuestionText(string text)
    {
        questionText.text = text;
    }
}
