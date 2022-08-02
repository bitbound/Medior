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
using System.Drawing;

namespace Medior.Controls
{
    /// <summary>
    /// Interaction logic for RecordingFrameWindow.xaml
    /// </summary>
    public partial class RecordingFrameWindow : Window
    {
        public RecordingFrameWindow()
        {
            InitializeComponent();
        }

        public RecordingFrameWindow(Rectangle selectedArea)
        {
            SelectedArea = selectedArea;
            InitializeComponent();
        }

        public Rectangle SelectedArea { get; }
    }
}
