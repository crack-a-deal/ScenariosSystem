
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITestService
{
    Task<TestDTO?> GetTestAsync();

    Task<TestDTO?> GetTestByIdAsync(Guid testId);

    Task<List<TestDTO>> GetAllTestsAsync();

    Task DeleteTestAsync(Guid testId);
    Task<List<TestListItemDTO>> GetAvailableTestsAsync();
    Task SaveAsync(TestDTO test);
}
