using Microsoft.AspNetCore.Components;
using Medior.Web.Client.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Medior.Web.Client.Services
{
    public interface IModalService
    {
        event EventHandler ModalClosed;

        event EventHandler ModalShown;
        string[]? Body { get; }
        List<ModalButton> Buttons { get; }
        RenderFragment? RenderBody { get; }
        string? Title { get; }
        void CloseModal();

        Task ShowModal(string title, string[] body, ModalButton[]? buttons = null);
        Task ShowModal(string title, RenderFragment body, ModalButton[]? buttons = null);
    }

    public class ModalService : IModalService
    {
        private readonly SemaphoreSlim _modalLock = new(1, 1);

        public event EventHandler? ModalClosed;

        public event EventHandler? ModalShown;
        public string[]? Body { get; private set; }
        public List<ModalButton> Buttons { get; } = new List<ModalButton>();
        public RenderFragment? RenderBody { get; private set; }
        public bool ShowInput { get; private set; }
        public string? Title { get; private set; }
        public void CloseModal()
        {
            ModalClosed?.Invoke(this, EventArgs.Empty);
        }

        public async Task ShowModal(string title, string[] body, ModalButton[]? buttons = null)
        {
            try
            {
                await _modalLock.WaitAsync();
                Title = title;
                Body = body;
                RenderBody = null;
                Buttons.Clear();
                if (buttons is not null)
                {
                    Buttons.AddRange(buttons);
                }
                ModalShown?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                _modalLock.Release();
            }
        }

        public async Task ShowModal(string title, RenderFragment body, ModalButton[]? buttons = null)
        {
            try
            {
                await _modalLock.WaitAsync();
                Title = title;
                RenderBody = body;
                Body = null;
                Buttons.Clear();
                if (buttons is not null)
                {
                    Buttons.AddRange(buttons);
                }
                ModalShown?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                _modalLock.Release();
            }
        }
    }
}
