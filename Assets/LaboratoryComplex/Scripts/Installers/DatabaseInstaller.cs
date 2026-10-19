using Zenject;

public class DatabaseInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<AppDbContext>()
            .FromMethod(_ =>
            {
                AppDbContext context = new AppDbContext("Education.db");
                context.Database.EnsureCreated();
                return context;
            })
            .AsSingle();
    }
}
