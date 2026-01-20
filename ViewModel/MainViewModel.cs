using System.ComponentModel;
using System.Runtime.CompilerServices;

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
    public MainViewModel()
    {

    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}