public class TestExcelMapper
{
    public TestExcelDto Map(TestDTO dto)
    {
        return new TestExcelDto
        {
            Title = dto.Title
        };
    }
}
