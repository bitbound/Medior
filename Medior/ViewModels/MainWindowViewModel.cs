using Medior.Reactive;
using Medior.Services;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Medior.ViewModels
{
    internal interface IShellViewModel
    {
        bool IsLoaderVisible { get; set; }

        string LoaderText { get; set; }
    }

    internal class ShellViewModel : ObservableObjectEx, IShellViewModel
    {
        private readonly ILoaderService _loader;
        public ShellViewModel(ILoaderService loader)
        {
            _loader = loader;

            _loader.LoaderShown += Loader_LoaderShown;
            _loader.LoaderHidden += Loader_LoaderHidden;
        }

        public bool IsLoaderVisible
        {
            get => Get<bool>();
            set => Set(value);
        }

        public string LoaderText
        {
            get => Get<string>() ?? string.Empty;
            set => Set(value);
        }

        private void Loader_LoaderHidden(object? sender, EventArgs e)
        {
            IsLoaderVisible = false;
            LoaderText = string.Empty;
        }

        private void Loader_LoaderShown(object? sender, string message)
        {
            LoaderText = message;
            IsLoaderVisible = true;
        }
    }
}
