public class CreateUpdateUserGroupDto
{
    public string Name { get; }
    public int Number { get; }

    public CreateUpdateUserGroupDto(
        string name,
        int number)
    {
        Name = name;
        Number = number;
    }
}