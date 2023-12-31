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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tools.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Tools.Views.Pages
{
    /// <summary>
    /// Interaction logic for HostFileProxy.xaml
    /// </summary>
    public partial class HostFileProxyPage : INavigableView<HostFileProxyViewModel>
    {
        public HostFileProxyViewModel ViewModel { get; }

        public HostFileProxyPage(HostFileProxyViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}