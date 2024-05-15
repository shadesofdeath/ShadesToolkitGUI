using Microsoft.Dism;
using ShadesToolkit.Helpers;
using ShadesToolkit.ViewModels.Pages;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Wpf.Ui.Controls;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.DirectoryServices;
using System.Threading.Tasks;
using MessageBoxButton = System.Windows.MessageBoxButton;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace ShadesToolkit.Views.Pages
{
    public partial class Components : INavigableView<ComponentsViewModel>
    {
        public ComponentsViewModel ViewModel { get; }

        public Components(ComponentsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}