// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// AddWords ViewModel, VM for a simple dialog box to enter words to add to current layout
//
// 2017-08-05   PV      First version
// 2023-11-20   PV      Net8 C#12
// 2024-11-15	PV		Net9 C#13

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

using Bonza.Editor.Support;

namespace Bonza.Editor.ViewModel;

internal sealed class AddWordsViewModel: INotifyPropertyChanged
{
    // Access to View and model (EditorModel, not AddWordsModel that doesn't exist)
    private readonly View.AddWordsView view;

    private readonly Model.EditorModel model;

    // Implementation of INotifyPropertyChanged, standard since View is only linked through DataBinding
    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Commands
    public ICommand OkCommand { get; }

    public ICommand CancelCommand { get; }

    public AddWordsViewModel(View.AddWordsView view, Model.EditorModel model)
    {
        // Initialize Model and ViewModel
        this.view = view;
        this.model = model;

        // Binding commands with behavior
        OkCommand = new RelayCommand<object>(OkExecute, OkCanExecute);
        CancelCommand = new RelayCommand<object>(CancelExecute);
    }

    // -------------------------------------------------
    // Bindings

    private string m_InputText;

    public string InputText
    {
        get => m_InputText;
        set
        {
            if (m_InputText != value)
            {
                m_InputText = value;
                NotifyPropertyChanged(nameof(InputText));

                PrepareAndValidateWordList();
            }
        }
    }

    private string m_StatusText;

    public string StatusText
    {
        get => m_StatusText;
        set
        {
            if (m_StatusText != value)
            {
                m_StatusText = value;
                NotifyPropertyChanged(nameof(StatusText));
            }
        }
    }

    private readonly List<string> wordsList = [];

    private void PrepareAndValidateWordList()
    {
        wordsList.Clear();
        foreach (var w in m_InputText.Split('\n'))
            if (!string.IsNullOrWhiteSpace(w.Trim()))
                wordsList.Add(w.Trim());

        if (wordsList.Count == 0)
        {
            StatusText = "Liste vide";
            return;
        }

        StatusText = model.CheckWordsList(wordsList);
    }

    // -------------------------------------------------
    // Commands

    private bool OkCanExecute(object obj) => wordsList.Count > 0 && string.IsNullOrEmpty(StatusText);

    private void OkExecute(object obj)
    {
        view.wordsList = wordsList;
        view.Close();
    }

    private void CancelExecute(object obj) => view.Close();
}
