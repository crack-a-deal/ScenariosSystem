using System.Collections.Generic;
using System.Linq;

public class ViewedModelExcelMapper
{
    public List<ViewedModelExcelDto> Map(
        IEnumerable<UserModelViewDto> views)
    {
        return views
            .Select((view, index) => new ViewedModelExcelDto
            {
                Number = index + 1,
                ModelName = view.ModelName,
                ViewedAt = view.ViewedAt
            })
            .ToList();
    }
}