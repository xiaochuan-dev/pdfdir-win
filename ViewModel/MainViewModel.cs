using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

public class MainViewModel : INotifyPropertyChanged
{
    private FileInfo _currentFile;

    public FileInfo CurrentFile
    {
        get => _currentFile;
        set
        {
            if (_currentFile != value)
            {
                _currentFile = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FileName));
            }
        }
    }

    public string FileName => CurrentFile?.Name ?? "未选择文件";

    private BookViewModel _bookDirectory;
    public BookViewModel BookDirectory
    {
        get => _bookDirectory;
        set
        {
            _bookDirectory = value;
            OnPropertyChanged();
        }
    }

    private string _inputText;

    public string InputText
    {
        get => _inputText;
        set
        {
            if (_inputText != value)
            {
                _inputText = value;
                OnPropertyChanged();
                OnInputChanged(value);
            }
        }
    }

    private void OnInputChanged(string newValue)
    {
        var res = BookChapterParser.Parse(newValue);
        if (res.Count != 0)
        {
            BookDirectory = new BookViewModel(res);
        }
    }

    public MainViewModel()
    {

    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}