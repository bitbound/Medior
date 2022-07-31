using Medior.Web.Client.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Timers;

namespace Medior.Web.Client.Services
{
    public interface IToastService
    {
        ObservableCollection<Toast> Toasts { get; }

        event EventHandler OnToastsChanged;

        void Show(string message, ToastType type = ToastType.Info, int expirationMillisecond = 3000, string? styleOverrides = null);
    }

    public class ToastService : IToastService
    {
        public event EventHandler? OnToastsChanged;
        public ObservableCollection<Toast> Toasts { get; } = new();

        public void Show(string message,
            ToastType type,
            int expirationMillisecond = 3000,
            string? styleOverrides = null)
        {
            var classString = type switch
            {
                ToastType.Info => "bg-info",
                ToastType.Success => "bg-success",
                ToastType.Warning => "bg-warning",
                ToastType.Error => "bg-danger",
                _ => "big-info"
            };

            classString += " text-white";

            var toastModel = new Toast(Guid.NewGuid().ToString(),
                message,
                classString,
                TimeSpan.FromMilliseconds(expirationMillisecond),
                styleOverrides);

            Toasts.Add(toastModel);

            OnToastsChanged?.Invoke(this, EventArgs.Empty);

            var removeToastTimer = new System.Timers.Timer(toastModel.Expiration.TotalMilliseconds + 1000)
            {
                AutoReset = false
            };
            removeToastTimer.Elapsed += (s, e) =>
            {
                Toasts.Remove(toastModel);
                OnToastsChanged?.Invoke(this, EventArgs.Empty);
                removeToastTimer.Dispose();
            };
            removeToastTimer.Start();
        }
    }
}
