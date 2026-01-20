using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Navigation;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

public class BookChapterExporter
{
    /// <summary>
    /// 读取PDF并添加书签
    /// </summary>
    /// <param name="inputpath">输入PDF文件路径</param>
    /// <param name="outputpath">输出PDF文件路径</param>
    /// <param name="chapters">章节集合</param>
    public static void WriteChaptersToPdf(string inputpath, string outputpath,
                                   ObservableCollection<BookChapter> chapters, int offset)
    {
        using (PdfReader reader = new PdfReader(inputpath))
        using (PdfWriter writer = new PdfWriter(outputpath))
        {
            PdfDocument pdfDoc = new PdfDocument(reader, writer);
            RemoveExistingBookmarks(pdfDoc);
            var outlines = pdfDoc.GetOutlines(false);
            AddBookmarks(outlines, chapters, pdfDoc, offset);
            pdfDoc.Close();
        }
    }
    public static void WriteChaptersToPdf(string path, ObservableCollection<BookChapter> chapters, int offset)
    {
        string tempFile = GetTempFilePath(path);
        
        try
        {
            using (PdfReader reader = new PdfReader(path))
            using (PdfWriter writer = new PdfWriter(tempFile))
            {
                PdfDocument pdfDoc = new PdfDocument(reader, writer);
                RemoveExistingBookmarks(pdfDoc);
                var outlines = pdfDoc.GetOutlines(false);
                AddBookmarks(outlines, chapters, pdfDoc, offset);
                pdfDoc.Close();
            }
            File.Delete(path);
            File.Move(tempFile, path);
        }
        catch
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
            throw;
        }
    }
    
    /// <summary>
    /// 获取临时文件路径
    /// </summary>
    private static string GetTempFilePath(string originalPath)
    {
        string directory = Path.GetDirectoryName(originalPath);
        string fileName = Path.GetFileNameWithoutExtension(originalPath);
        string extension = Path.GetExtension(originalPath);
        
        // 生成唯一的临时文件名
        string tempName = $"{fileName}_temp_{Guid.NewGuid():N}{extension}";
        
        return Path.Combine(directory ?? Path.GetTempPath(), tempName);
    }

    /// <summary>
    /// 添加书签
    /// </summary>
    private static void AddBookmarks(PdfOutline parentOutline,
                              ObservableCollection<BookChapter> chapters,
                              PdfDocument pdfDoc,
                              int offset)
    {
        if (chapters == null || !chapters.Any())
            return;

        foreach (var chapter in chapters)
        {
            if (chapter == null)
                continue;

            var chapterOutline = parentOutline.AddOutline(chapter.Title);

            if (chapter.PageNumber.HasValue)
            {
                int pageNum = chapter.PageNumber.Value + offset;

                if (pageNum > 0 && pageNum <= pdfDoc.GetNumberOfPages())
                {
                    chapterOutline.AddDestination(
                        PdfExplicitDestination.CreateFit(pdfDoc.GetPage(pageNum))
                    );
                }
            }

            if (chapter.Children != null && chapter.Children.Any())
            {
                AddBookmarks(chapterOutline, chapter.Children, pdfDoc, offset);
            }
        }
    }

    private static void RemoveExistingBookmarks(PdfDocument pdfDoc)
    {
        var catalog = pdfDoc.GetCatalog();
        var outlinesDict = catalog.GetPdfObject().GetAsDictionary(PdfName.Outlines);
        
        if (outlinesDict != null)
        {
            // 移除整个书签结构
            catalog.GetPdfObject().Remove(PdfName.Outlines);
            catalog.SetModified();
        }
    }
}