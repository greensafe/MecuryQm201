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
using System.Windows.Shapes;

namespace SilverTest
{
    /// <summary>
    /// CommandPanelWnd.xaml 的交互逻辑
    /// </summary>
    public partial class CommandPanelWnd : Window
    {
        public CommandPanelWnd()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("hello");
        }

        private void grabSampleBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
