using System.Collections.ObjectModel;

public class BookViewModel
{
    public ObservableCollection<BookChapter> Chapters { get; }
        = new ObservableCollection<BookChapter>();

    public BookViewModel(ObservableCollection<BookChapter> _chapters)
    {
        Chapters = _chapters;
    }
}
