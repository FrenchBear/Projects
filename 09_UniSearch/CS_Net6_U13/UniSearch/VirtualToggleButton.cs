// VirtualToggleButton.cs
// Helper for a WPF TreeView that enables automatic updates or child and hierarchy when a button is clicked

using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace UniSearch.Controls;

public static class VirtualToggleButton
{
    /// <summary>
    /// IsChecked Attached Dependency Property
    /// </summary>
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.RegisterAttached("IsChecked", typeof(bool?), typeof(VirtualToggleButton),
            new FrameworkPropertyMetadata((bool?)false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                OnIsCheckedChanged));

    /// <summary>
    /// Gets the IsChecked property.  This dependency property 
    /// indicates whether the toggle button is checked.
    /// </summary>
    public static bool? GetIsChecked(DependencyObject d)
        => (bool?)d?.GetValue(IsCheckedProperty);

    /// <summary>
    /// Sets the IsChecked property.  This dependency property 
    /// indicates whether the toggle button is checked.
    /// </summary>
    public static void SetIsChecked(DependencyObject d, bool? value)
        => d?.SetValue(IsCheckedProperty, value);

    /// <summary>
    /// Handles changes to the IsChecked property.
    /// </summary>
    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement pseudobutton)
        {
            bool? newValue = (bool?)e.NewValue;
            if (newValue == true)
            {
                _ = RaiseCheckedEvent(pseudobutton);
            }
            else if (newValue == false)
            {
                _ = RaiseUncheckedEvent(pseudobutton);
            }
            else
            {
                _ = RaiseIndeterminateEvent(pseudobutton);
            }
        }
    }

    /// <summary>
    /// IsThreeState Attached Dependency Property
    /// </summary>
    public static readonly DependencyProperty IsThreeStateProperty =
        DependencyProperty.RegisterAttached("IsThreeState", typeof(bool), typeof(VirtualToggleButton), new FrameworkPropertyMetadata(false));

    /// <summary>
    /// Gets the IsThreeState property.  This dependency property 
    /// indicates whether the control supports two or three states.  
    /// IsChecked can be set to null as a third state when IsThreeState is true.
    /// </summary>
    public static bool GetIsThreeState(DependencyObject d)
        => (bool) (d?.GetValue(IsThreeStateProperty) ?? false);

    /// <summary>
    /// Sets the IsThreeState property.  This dependency property 
    /// indicates whether the control supports two or three states. 
    /// IsChecked can be set to null as a third state when IsThreeState is true.
    /// </summary>
    public static void SetIsThreeState(DependencyObject d, bool value)
        => d?.SetValue(IsThreeStateProperty, value);

    /// <summary>
    /// IsVirtualToggleButton Attached Dependency Property
    /// </summary>
    public static readonly DependencyProperty IsVirtualToggleButtonProperty =
        DependencyProperty.RegisterAttached("IsVirtualToggleButton", typeof(bool), typeof(VirtualToggleButton),
            new FrameworkPropertyMetadata(false, OnIsVirtualToggleButtonChanged));

    /// <summary>
    /// Gets the IsVirtualToggleButton property.  This dependency property 
    /// indicates whether the object to which the property is attached is treated as a VirtualToggleButton.  
    /// If true, the object will respond to keyboard and mouse input the same way a ToggleButton would.
    /// </summary>
    public static bool GetIsVirtualToggleButton(DependencyObject d)
        => (bool)(d?.GetValue(IsVirtualToggleButtonProperty) ?? false);

    /// <summary>
    /// Sets the IsVirtualToggleButton property.  This dependency property 
    /// indicates whether the object to which the property is attached is treated as a VirtualToggleButton.  
    /// If true, the object will respond to keyboard and mouse input the same way a ToggleButton would.
    /// </summary>
    public static void SetIsVirtualToggleButton(DependencyObject d, bool value)
        => d?.SetValue(IsVirtualToggleButtonProperty, value);

    /// <summary>
    /// Handles changes to the IsVirtualToggleButton property.
    /// </summary>
    private static void OnIsVirtualToggleButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IInputElement element)
        {
            if ((bool)e.NewValue)
            {
                element.MouseLeftButtonDown += OnMouseLeftButtonDown;
                element.KeyDown += OnKeyDown;
            }
            else
            {
                element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                element.KeyDown -= OnKeyDown;
            }
        }
    }

    /// <summary>
    /// A static helper method to raise the Checked event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseCheckedEvent(UIElement target)
    {
        if (target == null) return null;

        var args = new RoutedEventArgs
        {
            RoutedEvent = ToggleButton.CheckedEvent
        };
        RaiseEvent(target, args);
        return args;
    }

    /// <summary>
    /// A static helper method to raise the Unchecked event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseUncheckedEvent(UIElement target)
    {
        if (target == null) return null;

        var args = new RoutedEventArgs
        {
            RoutedEvent = ToggleButton.UncheckedEvent
        };
        RaiseEvent(target, args);
        return args;
    }

    /// <summary>
    /// A static helper method to raise the Indeterminate event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseIndeterminateEvent(UIElement target)
    {
        if (target == null) return null;

        var args = new RoutedEventArgs
        {
            RoutedEvent = ToggleButton.IndeterminateEvent
        };
        RaiseEvent(target, args);
        return args;
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        UpdateIsChecked(sender as DependencyObject);
    }

    private static void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.OriginalSource == sender)
        {
            if (e.Key == Key.Space)
            {
                // ignore alt+space which invokes the system menu
                if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) return;

                UpdateIsChecked(sender as DependencyObject);
                e.Handled = true;

            }
            else if (e.Key == Key.Enter && (bool)((sender as DependencyObject)?.GetValue(KeyboardNavigation.AcceptsReturnProperty) ?? false))
            {
                UpdateIsChecked(sender as DependencyObject);
                e.Handled = true;
            }
        }
    }

    private static void UpdateIsChecked(DependencyObject d)
    {
        bool? isChecked = GetIsChecked(d);
        if (isChecked == true)
            SetIsChecked(d, GetIsThreeState(d) ? null : false);
        else
            SetIsChecked(d, isChecked.HasValue);
    }

    private static void RaiseEvent(DependencyObject target, RoutedEventArgs args)
    {
        if (target is UIElement e)
            e.RaiseEvent(args);
        else if (target is ContentElement c)
            c.RaiseEvent(args);
    }

}