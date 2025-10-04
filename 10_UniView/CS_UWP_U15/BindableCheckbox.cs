// UWP BindableCheckBox supporting three state (binding to Value object property)
// From https://stackoverflow.com/questions/33137798/3-state-checkbox-binding-in-mvvm-unable-to-set-null-state
// Binding to nullable types is not supported in WinRT
//
// 2023-08-24   PV

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace UniViewNS;

/// <summary>
/// Check Box Control with support for binding with an indeterminate state
/// </summary>
/// <remarks>https://www.danrigby.com/2012/07/24/windows-8-dev-tip-nullable-dependency-properties-and-binding/</remarks>
public class BindableCheckBox: CheckBox
{
    public BindableCheckBox()
    {
        IsThreeState = true;
        // start in the nullable state!
        IsChecked = null;

        // hook up UI events to manually set the value appropriately
        Checked += BindableCheckBox_Checked;
        Unchecked += BindableCheckBox_Unchecked;
        Indeterminate += BindableCheckBox_Indeterminate;

        // Hookup property changed event, in case some moron sets the IsChecked some other way
        RegisterPropertyChangedCallback(CheckBox.IsCheckedProperty, (s, e) =>
        {
            if (s is BindableCheckBox cb)
            {
                if (!cb._ValueIsChanging)
                {
                    var newValue = (bool?)s.GetValue(e);
                    if (cb.Value != newValue)               // Not 100% sure, in StackOverflow code, it's accessing State member instead of Value...
                        cb.Value = newValue;
                }
            }
        });
    }

    private bool _ValueIsChanging = false;
    private void BindableCheckBox_Indeterminate(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        if (!_ValueIsChanging)
            Value = null;
    }

    private void BindableCheckBox_Unchecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        if (!_ValueIsChanging)
            Value = false;
    }

    private void BindableCheckBox_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        if (!_ValueIsChanging)
            Value = true;
    }

    /// <summary>
    /// DP For the Three States of this checkbox, Null implies that it is in the Indeterminate state
    /// </summary>
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
           nameof(Value),
           typeof(object),
           typeof(BindableCheckBox), new PropertyMetadata(null, (s, e) =>
           {
               if (s is BindableCheckBox cb)
               {
                   try
                   {
                       cb._ValueIsChanging = true;
                       var newValue = (bool?)e.NewValue;
                       if (cb.IsChecked != newValue)
                           cb.IsChecked = newValue;
                   }
                   finally
                   {
                       cb._ValueIsChanging = false;
                   }
               }
           }));

    /// <summary>
    /// Value property, a bindable implementation of IsChecked
    /// </summary>
    /// <remarks>You should bind this property INSTEAD of IsChecked, if you want to bind to the indeterminate state</remarks>
    public bool? Value
    {
        get => (bool?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}