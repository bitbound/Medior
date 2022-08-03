﻿using System;
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
using System.Windows.Forms;

namespace Medior.Controls.ScreenCapture
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Left = Math.Max(SystemInformation.VirtualScreen.Left, SelectedArea.Left - RecordingFrame.StrokeThickness);
            Top = Math.Max(SystemInformation.VirtualScreen.Top, SelectedArea.Top - RecordingFrame.StrokeThickness);
            Width = SelectedArea.Width + (RecordingFrame.StrokeThickness * 2) + 1;
            Height = SelectedArea.Height + (RecordingFrame.StrokeThickness * 2) + 1;
        }
    }
}
