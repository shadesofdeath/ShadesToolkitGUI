﻿#pragma checksum "..\..\..\..\..\Views\Pages\Source.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "160104FBF9182E8A1CFB91168EB88085A2DFCC6F"
//------------------------------------------------------------------------------
// <auto-generated>
//     Bu kod araç tarafından oluşturuldu.
//     Çalışma Zamanı Sürümü:4.0.30319.42000
//
//     Bu dosyada yapılacak değişiklikler yanlış davranışa neden olabilir ve
//     kod yeniden oluşturulursa kaybolur.
// </auto-generated>
//------------------------------------------------------------------------------

using ShadesToolkit.Views.Pages;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Converters;
using Wpf.Ui.Markup;


namespace ShadesToolkit.Views.Pages {
    
    
    /// <summary>
    /// Source
    /// </summary>
    public partial class Source : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 22 "..\..\..\..\..\Views\Pages\Source.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.Button add_btn;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\..\..\Views\Pages\Source.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ContextMenu contextMenu;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\..\..\..\Views\Pages\Source.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.Button convert_btn;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\..\..\Views\Pages\Source.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.Button host_btn;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\..\..\Views\Pages\Source.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel CleanupMountsPanel;
        
        #line default
        #line hidden
        
        
        #line 50 "..\..\..\..\..\Views\Pages\Source.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.ProgressRing DataProgress;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\..\..\..\Views\Pages\Source.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.TextBlock DataText;
        
        #line default
        #line hidden
        
        
        #line 58 "..\..\..\..\..\Views\Pages\Source.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.DataGrid sourceDataGrid;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\..\..\..\Views\Pages\Source.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ContextMenu contextMenuControl;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.1.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ShadesToolkit;V1.0.0.0;component/views/pages/source.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\Views\Pages\Source.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.1.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.add_btn = ((Wpf.Ui.Controls.Button)(target));
            
            #line 22 "..\..\..\..\..\Views\Pages\Source.xaml"
            this.add_btn.Click += new System.Windows.RoutedEventHandler(this.Add_Btn_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.contextMenu = ((System.Windows.Controls.ContextMenu)(target));
            return;
            case 3:
            
            #line 25 "..\..\..\..\..\Views\Pages\Source.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.AddWim_ClickAsync);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 26 "..\..\..\..\..\Views\Pages\Source.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.AddIso_ClickAsync);
            
            #line default
            #line hidden
            return;
            case 5:
            this.convert_btn = ((Wpf.Ui.Controls.Button)(target));
            
            #line 35 "..\..\..\..\..\Views\Pages\Source.xaml"
            this.convert_btn.Click += new System.Windows.RoutedEventHandler(this.convert_btn_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.host_btn = ((Wpf.Ui.Controls.Button)(target));
            
            #line 42 "..\..\..\..\..\Views\Pages\Source.xaml"
            this.host_btn.Click += new System.Windows.RoutedEventHandler(this.host_btn_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.CleanupMountsPanel = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 8:
            this.DataProgress = ((Wpf.Ui.Controls.ProgressRing)(target));
            return;
            case 9:
            this.DataText = ((Wpf.Ui.Controls.TextBlock)(target));
            return;
            case 10:
            this.sourceDataGrid = ((Wpf.Ui.Controls.DataGrid)(target));
            
            #line 58 "..\..\..\..\..\Views\Pages\Source.xaml"
            this.sourceDataGrid.ContextMenuOpening += new System.Windows.Controls.ContextMenuEventHandler(this.sourceDataGrid_ContextMenuOpening);
            
            #line default
            #line hidden
            return;
            case 11:
            this.contextMenuControl = ((System.Windows.Controls.ContextMenu)(target));
            return;
            case 12:
            
            #line 62 "..\..\..\..\..\Views\Pages\Source.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.Mount_ClickAsync);
            
            #line default
            #line hidden
            return;
            case 13:
            
            #line 67 "..\..\..\..\..\Views\Pages\Source.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.Unmount_ClickAsync);
            
            #line default
            #line hidden
            return;
            case 14:
            
            #line 72 "..\..\..\..\..\Views\Pages\Source.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.Forget_ClickAsync);
            
            #line default
            #line hidden
            return;
            case 15:
            
            #line 77 "..\..\..\..\..\Views\Pages\Source.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.OpenFileDirectory_ClickAsync);
            
            #line default
            #line hidden
            return;
            case 16:
            
            #line 82 "..\..\..\..\..\Views\Pages\Source.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.OpenMountDirectory_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

