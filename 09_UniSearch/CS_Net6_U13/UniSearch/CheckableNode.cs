// CheckableNode.cs
// Implementation of a visual class to be used as node in a TreeView
// Original code from internet, forgot to record source address, but since I've found many copies...
// 2018-09-23   PV
// 2021-01-05   PV      RepresentantCharacter, LRFIconVisibility

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

#nullable enable

namespace UniSearch;

public class CheckableNode : INotifyPropertyChanged
{
    // Private variables
    private bool? _IsChecked = false;
    private CheckableNode? _Parent;

    // INotifyPropertyChanged interface
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // Constructor
    public CheckableNode(string name, int level, string? representantCharacter)
    {
        Name = name;
        Level = level;
        RepresentantCharacter = representantCharacter;
        Children = new List<CheckableNode>();
    }

    public void Initialize()
    {
        foreach (CheckableNode child in Children)
        {
            child._Parent = this;
            child.Initialize();
        }
    }

    public IList<CheckableNode> Children { get; }

    public bool IsInitiallySelected { get; private set; }

    private readonly string _Name = "";
    public string Name
    {
        get => _Name;
        init
        {
            if (_Name != value)
            {
                _Name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }
    }

    public string? RepresentantCharacter { get; }

    public Visibility LRFIconVisibility
        => Level == 0 ? Visibility.Visible : Visibility.Collapsed;

    private readonly int _Level;
    public int Level
    {
        get => _Level;
        init
        {
            if (_Level != value)
            {
                _Level = value;
                NotifyPropertyChanged(nameof(Level));
            }
        }
    }

    // Do not rename this property IsExpanded, disrupts binding since IsExpanded already exists
    private bool _IsExpanded = true;
    public bool IsNodeExpanded
    {
        get => _IsExpanded;
        set
        {
            if (_IsExpanded != value)
            {
                _IsExpanded = value;
                NotifyPropertyChanged(nameof(IsNodeExpanded));
            }
        }
    }

    /// <summary>
    /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
    /// The return value is calculated based on the check state of all
    /// child CheckableNodes.  Setting this property to true or false
    /// will set all children to the same check state, and setting it 
    /// to any value will cause the parent to verify its check state.
    /// </summary>
    public bool? IsChecked
    {
        get => _IsChecked;
        set => SetIsChecked(value, true, true);
    }
        

    void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
    {
        if (value == _IsChecked)
            return;

        _IsChecked = value;

        if (updateChildren && _IsChecked.HasValue)
            foreach (var c in Children)
                c.SetIsChecked(_IsChecked, true, false);
        //Children.ForEach(c => c.SetIsChecked(_IsChecked, true, false));

        if (updateParent && _Parent != null)
            _Parent.VerifyCheckState();

        NotifyPropertyChanged(nameof(IsChecked));
    }

    void VerifyCheckState()
    {
        bool? state = null;
        for (int i = 0; i < Children.Count; ++i)
        {
            bool? current = Children[i].IsChecked;
            if (i == 0)
            {
                state = current;
            }
            else if (state != current)
            {
                state = null;
                break;
            }
        }
        SetIsChecked(state, false, true);
    }
}