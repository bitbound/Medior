using MahApps.Metro.IconPacks;
using Medior.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Extensions
{
    internal static class ServiceCollectionExtensions
    {

        internal static void AddAppModule<TViewModelInterface, TViewModelImpl, TControlType>(this ServiceCollection services, string label, PackIconBoxIconsKind iconKind)
            where TViewModelInterface : class
            where TViewModelImpl : class, TViewModelInterface
        {
            services.AddSingleton<TViewModelInterface, TViewModelImpl>();

            services.AddSingleton(new AppModule(
                label,
                new PackIconBoxIcons() { Kind = iconKind }, 
                typeof(TControlType),
                typeof(TViewModelInterface), 
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
}
