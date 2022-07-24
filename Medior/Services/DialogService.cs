using Medior.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services
{
    internal interface IDialogService
    {
        Task<TDialog> GetCurrentDialogAsync<TDialog>() where TDialog : BaseMetroDialog;
        Task HideMetroDialogAsync(BaseMetroDialog dialog, MetroDialogSettings? settings = null);
        Task<MessageDialogResult> ShowError(Exception ex);

        Task<string> ShowInputAsync(string title, string message, MetroDialogSettings? settings = null);
        Task<LoginDialogData> ShowLoginAsync(string title, string message, LoginDialogSettings? settings = null);
        Task<MessageDialogResult> ShowMessageAsync(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings? settings = null);
        Task ShowMetroDialogAsync(BaseMetroDialog dialog, MetroDialogSettings? settings = null);
        string ShowModalInputExternal(string title, string message, MetroDialogSettings? settings = null);
        LoginDialogData ShowModalLoginExternal(string title, string message, LoginDialogSettings? settings = null);
        MessageDialogResult ShowModalMessageExternal(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings? settings = null);
        Task<ProgressDialogController> ShowProgressAsync(string title, string message, bool isCancelable = false, MetroDialogSettings? settings = null);
    }

    internal class DialogService : IDialogService
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IShellViewModel _shellWindowVm;
        public DialogService(IShellViewModel shellWindowVm, IDialogCoordinator dialogCoordinator)
        {
            _shellWindowVm = shellWindowVm;
            _dialogCoordinator = dialogCoordinator;
        }

        public Task<TDialog> GetCurrentDialogAsync<TDialog>()
            where TDialog : BaseMetroDialog
        {
            return _dialogCoordinator.GetCurrentDialogAsync<TDialog>(_shellWindowVm);
        }

        public Task HideMetroDialogAsync(BaseMetroDialog dialog, MetroDialogSettings? settings = null)
        {
            return _dialogCoordinator.HideMetroDialogAsync(_shellWindowVm, dialog, settings);
        }

        public Task<MessageDialogResult> ShowError(Exception ex)
        {
            return _dialogCoordinator.ShowMessageAsync(_shellWindowVm, 
                "Oh darn.  An error.", 
                $"Here's what it said:\n\n{ex.Message}",
                MessageDialogStyle.Affirmative);
        }

        public Task<string> ShowInputAsync(string title, string message, MetroDialogSettings? settings = null)
        {
            return _dialogCoordinator.ShowInputAsync(_shellWindowVm, title, message, settings);
        }

        public Task<LoginDialogData> ShowLoginAsync(string title, string message, LoginDialogSettings? settings = null)
        {
            return _dialogCoordinator.ShowLoginAsync(_shellWindowVm, title, message, settings);
        }

        public Task<MessageDialogResult> ShowMessageAsync(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings? settings = null)
        {
            return _dialogCoordinator.ShowMessageAsync(_shellWindowVm, title, message, style, settings);
        }

        public Task ShowMetroDialogAsync(BaseMetroDialog dialog, MetroDialogSettings? settings = null)
        {
            return _dialogCoordinator.ShowMetroDialogAsync(_shellWindowVm, dialog, settings);
        }

        public string ShowModalInputExternal(string title, string message, MetroDialogSettings? settings = null)
        {
            return _dialogCoordinator.ShowModalInputExternal(_shellWindowVm, title, message, settings);
        }

        public LoginDialogData ShowModalLoginExternal(string title, string message, LoginDialogSettings? settings = null)
        {
            return _dialogCoordinator.ShowModalLoginExternal(_shellWindowVm, title, message, settings);
        }

        public MessageDialogResult ShowModalMessageExternal(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings? settings = null)
        {
            return _dialogCoordinator.ShowModalMessageExternal(_shellWindowVm, title, message, style, settings);
        }

        public Task<ProgressDialogController> ShowProgressAsync(string title, string message, bool isCancelable = false, MetroDialogSettings? settings = null)
        {
            return _dialogCoordinator.ShowProgressAsync(_shellWindowVm, title, message, isCancelable, settings);
        }
    }
}
