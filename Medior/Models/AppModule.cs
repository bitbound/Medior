using MahApps.Metro.IconPacks;
using System;

namespace Medior.Models;

public class AppModule
{
    public AppModule(string label, PackIconControlBase icon, Type controlType, Type? viewModelType = null)
    {
        Label = label;
        Icon = icon;
        ControlType = controlType;
        ViewModelType = viewModelType;
    }

    public Type ControlType { get; }
    public PackIconControlBase Icon { get; }
    public string Label { get; }
    public Type? ViewModelType { get; }
}
