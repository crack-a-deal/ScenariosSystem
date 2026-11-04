using Networking;
using Zenject;

public class NetworkingInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        NetworkingConfig config = NetworkingConfigLoader.Load();

        Container.Bind<NetworkingConfig>()
            .FromInstance(config)
            .AsSingle();

        Container.Bind<IMessageSerializer>()
            .To<JsonMessageSerializer>()
            .AsSingle();

        Container.Bind<ClientRegistry>().AsSingle();
        Container.Bind<TeacherNetworkingFactory>().AsSingle();

        Container.Bind<TcpServer>()
            .FromMethod(context =>
                context.Container.Resolve<TeacherNetworkingFactory>().Create())
            .AsSingle();

        Container.Bind<UdpDiscoveryListener>().AsSingle();
        Container.Bind<UdpDiscoveryService>().AsSingle();

        Container.Bind<TcpServerProvider>().AsSingle();
        Container.Bind<TcpClientProvider>().AsSingle();
    }
}
