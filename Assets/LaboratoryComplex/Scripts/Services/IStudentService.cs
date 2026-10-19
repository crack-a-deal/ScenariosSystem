using System.Collections.Generic;

public interface IStudentService
{
    IReadOnlyList<StudentDto> GetAll();
    public void Delete(int id);
}
