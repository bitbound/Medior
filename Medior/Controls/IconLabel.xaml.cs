using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Controls;

namespace Medior.Controls
{
    /// <summary>
    /// Interaction logic for IconLabel.xaml
    /// </summary>
    public partial class IconLabel : UserControl
    {
        public static readonly DependencyProperty IconTextProperty =
            DependencyProperty.Register("IconText", typeof(string), typeof(IconLabel), new PropertyMetadata());

        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register("IconKind", typeof(PackIconBoxIconsKind), typeof(IconLabel), new PropertyMetadata());


        public IconLabel()
        {
            InitializeComponent();
        }



        public string IconText
        {
            get { return (string)GetValue(IconTextProperty); }
            set { SetValue(IconTextProperty, value); }
        }

        public PackIconBoxIconsKind IconKind
        {
            get { return (PackIconBoxIconsKind)GetValue(IconKindProperty); }
            set { SetValue(IconKindProperty, value); }
        }
    }
}
