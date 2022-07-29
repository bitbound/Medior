using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Models
{
    internal class AppModule
    {
        public AppModule(string label, PackIconControlBase icon, Type controlType, Type? vmInterface = null, Type? vmImplementation = null)
        {
            Label = label;
            Icon = icon;
            ControlType = controlType;

            if (vmInterface is not null && vmImplementation is not null)
            {
                ViewModelInterfaceType = vmInterface;
                ViewModelImplementationType = vmImplementation;
            }

        }

        public Type ControlType { get; }
        public PackIconControlBase Icon { get; }
        public string Label { get; }
        public Type? ViewModelImplementationType { get; }
        public Type? ViewModelInterfaceType { get; }
    }
}
