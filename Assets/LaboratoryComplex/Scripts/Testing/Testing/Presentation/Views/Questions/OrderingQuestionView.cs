using TMPro;
using UnityEngine;

public class OrderingQuestionView : MonoBehaviour
{
    [SerializeField] private TMP_Text questionText;

    [SerializeField] private ReordableListView reordableList;

    public ReordableListView ReordableList => reordableList;

    public void SetQuestionText(string text)
    {
        questionText.text = text;
    }
}
