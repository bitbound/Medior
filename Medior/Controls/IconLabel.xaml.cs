using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
