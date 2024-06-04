using MahApps.Metro.IconPacks;
using Medior.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Medior.Extensions;

internal static class ServiceCollectionExtensions
{

    internal static void AddAppModule<TViewModelImpl, TControlType>(this ServiceCollection services, string label, PackIconBoxIconsKind iconKind)
        where TViewModelImpl : class
    {
        services.AddSingleton<TViewModelImpl>();

        services.AddSingleton(new AppModule(
            label,
            new PackIconBoxIcons() { Kind = iconKind }, 
            typeof(TControlType),
            typeof(TViewModelImpl)));

    }

    internal static void AddAppModule<TControlType>(this ServiceCollection services, string label, PackIconBoxIconsKind iconKind)
    {
        services.AddSingleton(new AppModule(
            label, 
            new PackIconBoxIcons() { Kind = iconKind },
            typeof(TControlType)));

    }
}
