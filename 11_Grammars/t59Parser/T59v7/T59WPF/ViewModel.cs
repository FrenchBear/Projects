// T59v7WPF ViewModel
//
// 2025-12-04   PV

using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace T59v7WPF;

internal sealed class ViewModel: INotifyPropertyChanged
{
    // Access to Model and window
    private readonly Model M;
    private readonly MainWindow W;

    // INotifyPropertyChanged interface
    public event PropertyChangedEventHandler? PropertyChanged;

    public void NotifyPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // Commands public interface
    public ICommand NewCommand { get; private set; }
    public ICommand OpenFileCommand { get; private set; }
    public ICommand SaveCommand { get; private set; }
    public ICommand SaveAsCommand { get; private set; }
    public ICommand CloseCommand { get; private set; }
    public ICommand AboutCommand { get; private set; }
    public ICommand TestColorsCommand { get; private set; }

    // ---------------------------------------------------------
    // Constructor

    public ViewModel(Model m, MainWindow w)
    {
        M = m;
        W = w;

        // Binding commands with behavior
        NewCommand = new RelayCommand<object>(NewExecute);
        OpenFileCommand = new RelayCommand<object>(OpenFileExecute);
        SaveCommand = new RelayCommand<object>(SaveExecute);
        SaveAsCommand = new RelayCommand<object>(SaveAsExecute);
        CloseCommand = new RelayCommand<object>(CloseExecute);
        AboutCommand = new RelayCommand<object>(AboutExecute);
        TestColorsCommand = new RelayCommand<object>(TestColorsExecute);
    }

    // ---------------------------------------------------------
    // Binding properties

    public string SourceCode
    {
        get;
        set
        {
            field = value;
            M.ProcessSourceCode(value);
            NotifyPropertyChanged(nameof(SourceCode));
        }
    } = "";

    public bool IsDirty
    {
        get; internal set
        {
            if (field != value)
            {
                field = value;
                W.UpdateTitle();
            }
        }
    }

    public string? FileName
    {
        get; internal set
        {
            field = value;
            W.UpdateTitle();
        }
    }

    public string StatusMessage
    {
        get; internal set
        {
            if (field != value)
            {
                field = value;
                NotifyPropertyChanged(nameof(StatusMessage));
            }
        }
    } = "";

    public string Timing1
    {
        get; internal set
        {
            if (field != value)
            {
                field = value;
                NotifyPropertyChanged(nameof(Timing1));
            }
        }
    } = "";

    public string Timing2
    {
        get; internal set
        {
            if (field != value)
            {
                field = value;
                NotifyPropertyChanged(nameof(Timing2));
            }
        }
    } = "";

    public string Timing3
    {
        get; internal set
        {
            if (field != value)
            {
                field = value;
                NotifyPropertyChanged(nameof(Timing3));
            }
        }
    } = "";

    public string ColorizedHtml
    {
        get;
        set
        {
            field = value;
            W.DisplayColorizedHtml(value);
        }
    } = "";

    public string ReformattedHtml
    {
        get;
        set
        {
            field = value;
            W.DisplayReformattedHtml(value);
        }
    } = "";

    // ---------------------------------------------------------
    // Command handlers
    internal bool CanContinue()
    {
        if (IsDirty)
        {
            var r = MessageBox.Show("Le programme a été modifié mais pas enregistré.\r\nVoulez-vous conserver ces changements?", "T59v7WPF", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.None);
            return MessageBoxResult.No == r;
        }
        else
            return true;
    }

    // -----------

    private void NewExecute(object obj)
    {
        if (!CanContinue())
            return;

        SourceCode = "";
        FileName = null;
        IsDirty = false;
        W.UpdateTitle();
    }

    // -----------

    internal void OpenFileExecute(object obj)
    {
        if (!CanContinue())
            return;

        // Configure open file dialog box
        Microsoft.Win32.OpenFileDialog dlg = new()
        {
            FileName = "", // Default file name
            DefaultExt = ".t59", // Default file extension
            Filter = "Ti 58C/59 programs|*.t59;*.src|Tous les fichiers (*.*)|*.*"
        };

        // Show open file dialog box
        bool? result = dlg.ShowDialog();

        // Process open file dialog box results
        if (result == true)
        {
            try
            {
                using StreamReader sr = new(dlg.FileName, Encoding.UTF8);
                SourceCode = sr.ReadToEnd();
                IsDirty = false;
                FileName = dlg.FileName;
                W.UpdateTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur est survenue lors de l'ouverture du fichier " + dlg.FileName + ": " + ex.Message, "T59v7WPF", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }

    // -----------

    private void SaveExecute(object obj)
    {
        if (FileName == null)
            SaveAsExecute(obj);
        else
        {
            try
            {
                using StreamWriter sw = new(FileName, false, Encoding.UTF8);
                sw.Write(SourceCode);
                IsDirty = false;
                W.UpdateTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur est survenue lors de l'écriture du fichier " + FileName + ": " + ex.Message, "T59v7WPF", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

    }

    // -----------

    private void SaveAsExecute(object obj)
    {
        // Configure save file dialog box
        Microsoft.Win32.SaveFileDialog dlg = new()
        {
            FileName = "", // Default file name
            DefaultExt = ".t59", // Default file extension
            Filter = "Ti 58C/59 programs|*.t59;*.src|Tous les fichiers (*.*)|*.*"  // Filter files by extension
        };

        // Show save file dialog box
        bool? result = dlg.ShowDialog();

        // Process save file dialog box results
        if (result == true)
        {
            // Save program
            FileName = dlg.FileName;
            SaveExecute(obj);
        }
    }

    // -----------

    private void CloseExecute(object obj)
        => W.Close();

    // -----------

    private void AboutExecute(object obj)
        => MessageBox.Show("Simple ANTRL4 lexer + my own grammar parser to validate, encode and colorize a TI-58C/59 program", MainWindow.AppName, MessageBoxButton.OK, MessageBoxImage.Information);

    // -----------

    private void TestColorsExecute(object obj)
    {
        string test = @"comment:     [comment]Text 123[/comment]
            invalid:     [invalid]Text 123[/invalid]
            unknown:     [unknown]Text 123[/unknown]
            instruction: [instruction]Text 123[/instruction]
            number:      [number]Text 123[/number]
            direct:      [direct]Text 123[/direct]
            indirect:    [indirect]Text 123[/indirect]
            tag:         [tag]Text 123[/tag]
            label:       [label]Text 123[/label]
            address:     [address]Text 123[/address]

            [comment]// Example[/comment]
            [tag]@Loop:[/tag] [number]-1.6E-19[/number] [instruction]PD*[/instruction] [indirect]04[/indirect] [invalid]ZYP[/invalid] [instruction]Dsz[/instruction] [direct]12[/direct] [label]CLR[/label]";
        var out2 = Model.HtmlRender(test);
        W.DisplayReformattedHtml(out2);
    }

}
