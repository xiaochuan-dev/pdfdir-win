using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class BookChapter : INotifyPropertyChanged
{
    private string _title;
    private int? _pageNumber;
    private ObservableCollection<BookChapter> _children;

    public string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(); }
    }

    public int? PageNumber
    {
        get => _pageNumber;
        set { _pageNumber = value; OnPropertyChanged(); }
    }

    public ObservableCollection<BookChapter> Children
    {
        get => _children ??= new ObservableCollection<BookChapter>();
        set { _children = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
