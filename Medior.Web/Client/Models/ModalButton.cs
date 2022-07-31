using System;

namespace Medior.Web.Client.Models
{
    public class ModalButton
    {
        public ModalButton(string cssClass, string text, Action clickCallback)
        {
            Class = cssClass;
            Text = text;
            OnClick = clickCallback;
        }

        public string Class { get; init; }
        public string Text { get; init; }

        public Action OnClick { get; init; }
    }
}
