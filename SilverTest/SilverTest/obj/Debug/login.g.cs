﻿#pragma checksum "..\..\login.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "D4FD50138F066C534C83802ACBB68E5FAE10EC7F"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using MahApps.Metro.IconPacks;
using SilverTest;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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


namespace SilverTest {
    
    
    /// <summary>
    /// login
    /// </summary>
    public partial class login : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 12 "..\..\login.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal SilverTest.login mainwindow;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\login.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grid;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\login.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel stackPanel;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\login.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox usertxt;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\login.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox passwordtxt;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/QM201H;component/login.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\login.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.mainwindow = ((SilverTest.login)(target));
            return;
            case 2:
            this.grid = ((System.Windows.Controls.Grid)(target));
            
            #line 18 "..\..\login.xaml"
            this.grid.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.grid_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 3:
            this.stackPanel = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 4:
            
            #line 30 "..\..\login.xaml"
            ((MahApps.Metro.IconPacks.PackIconMaterial)(target)).MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.PackIcon_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 5:
            this.usertxt = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            this.passwordtxt = ((System.Windows.Controls.PasswordBox)(target));
            return;
            case 7:
            
            #line 43 "..\..\login.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click_1);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 44 "..\..\login.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

