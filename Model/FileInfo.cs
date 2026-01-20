using System.ComponentModel;
using System.Runtime.CompilerServices;

public class FileInfo : INotifyPropertyChanged
{
    private string _path;
    private string _name;

    public string Path
    {
        get => _path;
        set
        {
            _path = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public FileInfo(string filename, string path)
    {
        _name = filename;
        _path = path;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}