﻿#pragma checksum "..\..\..\GraphTabControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "99F7F12ADC5B422204B856A8E1C8933E"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using DesignScriptStudio.Graph.Ui;
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


namespace DesignScriptStudio.Graph.Ui {
    
    
    /// <summary>
    /// GraphTabControl
    /// </summary>
    public partial class GraphTabControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 183 "..\..\..\GraphTabControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TabControl TabControl;
        
        #line default
        #line hidden
        
        
        #line 190 "..\..\..\GraphTabControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Menu AllTabsMenu;
        
        #line default
        #line hidden
        
        
        #line 410 "..\..\..\GraphTabControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem AllTabs;
        
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
            System.Uri resourceLocater = new System.Uri("/DesignScriptStudio.Graph.Ui;component/graphtabcontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\GraphTabControl.xaml"
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
            case 4:
            this.TabControl = ((System.Windows.Controls.TabControl)(target));
            
            #line 183 "..\..\..\GraphTabControl.xaml"
            this.TabControl.PreviewMouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.OnTabControlMouseDoubleClick);
            
            #line default
            #line hidden
            
            #line 183 "..\..\..\GraphTabControl.xaml"
            this.TabControl.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OnTabControlMouseUp);
            
            #line default
            #line hidden
            
            #line 183 "..\..\..\GraphTabControl.xaml"
            this.TabControl.MouseMove += new System.Windows.Input.MouseEventHandler(this.OnTabControlMouseMove);
            
            #line default
            #line hidden
            return;
            case 5:
            this.AllTabsMenu = ((System.Windows.Controls.Menu)(target));
            return;
            case 10:
            this.AllTabs = ((System.Windows.Controls.MenuItem)(target));
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            System.Windows.EventSetter eventSetter;
            switch (connectionId)
            {
            case 1:
            eventSetter = new System.Windows.EventSetter();
            eventSetter.Event = System.Windows.UIElement.PreviewMouseLeftButtonDownEvent;
            
            #line 72 "..\..\..\GraphTabControl.xaml"
            eventSetter.Handler = new System.Windows.Input.MouseButtonEventHandler(this.OnTabItemMouseDown);
            
            #line default
            #line hidden
            ((System.Windows.Style)(target)).Setters.Add(eventSetter);
            eventSetter = new System.Windows.EventSetter();
            eventSetter.Event = System.Windows.UIElement.PreviewMouseRightButtonUpEvent;
            
            #line 73 "..\..\..\GraphTabControl.xaml"
            eventSetter.Handler = new System.Windows.Input.MouseButtonEventHandler(this.OnTabItemRightClick);
            
            #line default
            #line hidden
            ((System.Windows.Style)(target)).Setters.Add(eventSetter);
            break;
            case 2:
            
            #line 114 "..\..\..\GraphTabControl.xaml"
            ((System.Windows.Controls.TextBox)(target)).PreviewLostKeyboardFocus += new System.Windows.Input.KeyboardFocusChangedEventHandler(this.OnRenameTextBoxLostFocus);
            
            #line default
            #line hidden
            
            #line 115 "..\..\..\GraphTabControl.xaml"
            ((System.Windows.Controls.TextBox)(target)).KeyDown += new System.Windows.Input.KeyEventHandler(this.OnRenameTextBoxKeyDown);
            
            #line default
            #line hidden
            break;
            case 3:
            
            #line 123 "..\..\..\GraphTabControl.xaml"
            ((System.Windows.Controls.Button)(target)).PreviewMouseUp += new System.Windows.Input.MouseButtonEventHandler(this.OnTabClose);
            
            #line default
            #line hidden
            break;
            case 6:
            
            #line 204 "..\..\..\GraphTabControl.xaml"
            ((System.Windows.Controls.Border)(target)).MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.OnMenuHeaderMouseDown);
            
            #line default
            #line hidden
            break;
            case 7:
            
            #line 276 "..\..\..\GraphTabControl.xaml"
            ((System.Windows.Controls.Border)(target)).MouseUp += new System.Windows.Input.MouseButtonEventHandler(this.OnAllTabMenuItemMouseUp);
            
            #line default
            #line hidden
            break;
            case 8:
            
            #line 286 "..\..\..\GraphTabControl.xaml"
            ((System.Windows.Controls.Button)(target)).PreviewMouseUp += new System.Windows.Input.MouseButtonEventHandler(this.OnAllTabCloseTabMouseUp);
            
            #line default
            #line hidden
            break;
            case 9:
            
            #line 337 "..\..\..\GraphTabControl.xaml"
            ((System.Windows.Controls.Button)(target)).PreviewMouseUp += new System.Windows.Input.MouseButtonEventHandler(this.OnAllTabSaveTabMouseUp);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}
