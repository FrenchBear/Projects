// CheckableNode.cs
// Implementation of a visual class to be used as node in a TreeView
// Original code from internet, forgot to record source address, but since I've found many copies...
// 2018-09-23   PV


using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace UniSearchNS
{
    public class CheckableNode : INotifyPropertyChanged
    {
        // Private variables
        private bool? _isChecked = false;
        private CheckableNode _parent;


        // INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        // Constructor
        public CheckableNode(string name, int level)
        {
            Name = name;
            Level = level;
            Children = new List<CheckableNode>();
        }

        public void Initialize()
        {
            foreach (CheckableNode child in Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }


        public IList<CheckableNode> Children { get; private set; }

        public bool IsInitiallySelected { get; private set; }


        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    NotifyPropertyChanged(nameof(Name));
                }
            }
        }

        private int _Level;
        public int Level
        {
            get { return _Level; }
            set
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
            get { return _IsExpanded; }
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
            get { return _isChecked; }
            set { SetIsChecked(value, true, true); }
        }
        

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
                foreach (var c in Children)
                    c.SetIsChecked(_isChecked, true, false);
                //Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent && _parent != null)
                _parent.VerifyCheckState();

            NotifyPropertyChanged("IsChecked");
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
}
