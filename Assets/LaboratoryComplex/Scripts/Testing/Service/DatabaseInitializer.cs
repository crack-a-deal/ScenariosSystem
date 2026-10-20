using System.Threading.Tasks;
using UnityEngine;

public class DatabaseInitializer
{
    private readonly AppDbContext _context;
    private readonly ThreeDModelService _modelService;
    private readonly ModulesList _modulesList;

    public DatabaseInitializer(AppDbContext context, ThreeDModelService modelService, ModulesList modulesList)
    {
        _context = context;
        _modelService = modelService;
        _modulesList = modulesList;
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        await _modelService.InitializeCatalogAsync(_modulesList.Modules);
        Debug.Log("База данных и каталог 3D-моделей готовы.");
    }
}