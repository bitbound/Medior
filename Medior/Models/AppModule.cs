using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Models
{
    internal class AppModule<TControlType>
    {
        public AppModule(string label, PackIconControlBase icon, Type? vmInterface = null, Type? vmImplementation = null)
        {
            Label = label;
            Icon = icon;

            if (vmInterface is not null && vmImplementation is not null)
            {
                ViewModelInterfaceType = vmInterface;
                ViewModelImplementationType = vmImplementation;
            }

            ControlType = typeof(TControlType);
        }

        public Type ControlType { get; }
        public PackIconControlBase Icon { get; }
        public string Label { get; }
        public Type? ViewModelImplementationType { get; }
        public Type? ViewModelInterfaceType { get; }
    }
}
