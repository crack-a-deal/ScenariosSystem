using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StudentFormView : MonoBehaviour
{
    public event Action SubmitClicked;
    public event Action CancelClicked;

    [SerializeField] private TMP_InputField loginInput;
    [SerializeField] private TMP_InputField passwordInput;


    [SerializeField] private TMP_InputField firstNameInput;
    [SerializeField] private TMP_InputField lastNameInput;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Dropdown groupDropdown;

    [SerializeField] private Button submitButton;
    [SerializeField] private Button cancelButton;

    private readonly List<Guid> _groupIds = new();

    private void Awake()
    {
        submitButton.onClick.AddListener(OnSubmitClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    private void OnDestroy()
    {
        submitButton.onClick.RemoveListener(OnSubmitClicked);
        cancelButton.onClick.RemoveListener(OnCancelClicked);
    }

    private void OnSubmitClicked()
    {
        SubmitClicked?.Invoke();
    }

    private void OnCancelClicked()
    {
        CancelClicked?.Invoke();
    }

    public string GetLogin()
    {
        return loginInput.text;
    }

    public void SetLogin(string login)
    {
        loginInput.text = login;
    }

    public string GetPassword()
    {
        return passwordInput.text;
    }

    public void SetPassword(string password)
    {
        passwordInput.text = password;
    }

    public string GetFirstName()
    {
        return firstNameInput.text;
    }

    public void SetFirstName(string firstName)
    {
        firstNameInput.text = firstName;
    }

    public string GetLastName()
    {
        return lastNameInput.text;
    }

    public void SetLastName(string lastName)
    {
        lastNameInput.text = lastName;
    }

    public string GetPatronymic()
    {
        return nameInput.text;
    }

    public void SetPatronymic(string name)
    {
        nameInput.text = name;
    }


    public void SetGroups(IReadOnlyList<UserGroupDto> groups)
    {
        _groupIds.Clear();
        groupDropdown.ClearOptions();

        var options = new List<TMP_Dropdown.OptionData>();

        foreach (UserGroupDto group in groups)
        {
            _groupIds.Add(group.Id);
            options.Add(new TMP_Dropdown.OptionData(group.Name));
        }

        groupDropdown.AddOptions(options);
        groupDropdown.interactable = options.Count > 0;

        if (options.Count > 0)
            groupDropdown.value = 0;

        groupDropdown.RefreshShownValue();
    }

    public Guid? GetSelectedGroupId()
    {
        int index = groupDropdown.value;

        if (index < 0 || index >= _groupIds.Count)
            return null;

        return _groupIds[index];
    }

    public void SelectGroup(Guid groupId)
    {
        int index = _groupIds.IndexOf(groupId);

        if (index >= 0)
            groupDropdown.value = index;

        groupDropdown.RefreshShownValue();
    }

    public void Clear()
    {
        loginInput.text = string.Empty;
        passwordInput.text = string.Empty;

        firstNameInput.text = string.Empty;
        lastNameInput.text = string.Empty;
        nameInput.text = string.Empty;
        groupDropdown.ClearOptions();
    }
}
