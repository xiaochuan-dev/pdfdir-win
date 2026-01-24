using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Navigation;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;

public class BookChapterExporter
{
    /// <summary>
    /// 读取PDF并添加书签（双参数版本）
    /// </summary>
    /// <param name="inputpath">输入PDF文件路径</param>
    /// <param name="outputpath">输出PDF文件路径</param>
    /// <param name="chapters">章节集合</param>
    /// <param name="offset">页码偏移量</param>
    public static void WriteChaptersToPdf(string inputpath, string outputpath,
                                          ObservableCollection<BookChapter> chapters, int offset)
    {
        string tempFile = GetTempFilePath(outputpath);

        try
        {
            using (var reader = new PdfReader(inputpath))
            using (var writer = new PdfWriter(tempFile))
            {
                var pdfDoc = new PdfDocument(reader, writer);

                // 移除现有书签
                var catalog = pdfDoc.GetCatalog();
                catalog.GetPdfObject().Remove(PdfName.Outlines);

                // 批量添加书签
                if (chapters?.Any() == true)
                {
                    var outlines = pdfDoc.GetOutlines(false);
                    AddBookmarksBatch(outlines, chapters, pdfDoc, offset);
                }

                pdfDoc.Close();
            }

            // 确保输出目录存在
            var outputDir = Path.GetDirectoryName(outputpath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // 复制临时文件到输出路径
            File.Copy(tempFile, outputpath, true);
            File.Delete(tempFile);
        }
        catch
        {
            SafeDelete(tempFile);
            throw;
        }
    }

    /// <summary>
    /// 读取PDF并添加书签（单参数版本 - 原地修改）
    /// </summary>
    /// <param name="path">PDF文件路径</param>
    /// <param name="chapters">章节集合</param>
    /// <param name="offset">页码偏移量</param>
    public static void WriteChaptersToPdf(string path, ObservableCollection<BookChapter> chapters, int offset)
    {
        string tempFile = GetTempFilePath(path);

        try
        {
            using (var reader = new PdfReader(path))
            using (var writer = new PdfWriter(tempFile))
            {
                var pdfDoc = new PdfDocument(reader, writer);

                // 移除现有书签
                var catalog = pdfDoc.GetCatalog();
                catalog.GetPdfObject().Remove(PdfName.Outlines);

                // 批量添加书签
                if (chapters?.Any() == true)
                {
                    var outlines = pdfDoc.GetOutlines(false);
                    AddBookmarksBatch(outlines, chapters, pdfDoc, offset);
                }

                pdfDoc.Close();
            }

            // 替换原文件
            File.Replace(tempFile, path, null);
        }
        catch
        {
            SafeDelete(tempFile);
            throw;
        }
    }

    /// <summary>
    /// 异步版本 - 显著提升响应速度
    /// </summary>
    public static async Task WriteChaptersToPdfAsync(string path,
        ObservableCollection<BookChapter> chapters, int offset)
    {
        await Task.Run(() => WriteChaptersToPdf(path, chapters, offset));
    }

    /// <summary>
    /// 异步版本 - 双参数重载
    /// </summary>
    public static async Task WriteChaptersToPdfAsync(string inputpath, string outputpath,
        ObservableCollection<BookChapter> chapters, int offset)
    {
        await Task.Run(() => WriteChaptersToPdf(inputpath, outputpath, chapters, offset));
    }

    /// <summary>
    /// 批量添加书签（使用栈减少递归开销）
    /// </summary>
    private static void AddBookmarksBatch(PdfOutline parentOutline,
        ObservableCollection<BookChapter> chapters,
        PdfDocument pdfDoc,
        int offset)
    {
        var stack = new System.Collections.Generic.Stack<(PdfOutline, ObservableCollection<BookChapter>)>();
        stack.Push((parentOutline, chapters));

        while (stack.Count > 0)
        {
            var (currentOutline, currentChapters) = stack.Pop();

            foreach (var chapter in currentChapters)
            {
                if (chapter == null) continue;

                var chapterOutline = currentOutline.AddOutline(chapter.Title);

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

                if (chapter.Children?.Any() == true)
                {
                    stack.Push((chapterOutline, chapter.Children));
                }
            }
        }
    }

    /// <summary>
    /// 获取临时文件路径
    /// </summary>
    private static string GetTempFilePath(string originalPath)
    {
        var directory = Path.GetDirectoryName(originalPath);
        var tempName = $"temp_{Guid.NewGuid():N}{Path.GetExtension(originalPath)}";
        return Path.Combine(directory ?? Path.GetTempPath(), tempName);
    }

    /// <summary>
    /// 安全删除文件
    /// </summary>
    private static void SafeDelete(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        catch
        {
            // 忽略删除错误，防止异常嵌套
        }
    }

    /// <summary>
    /// 可选：移除现有书签的原始方法（保持兼容性）
    /// </summary>
    private static void RemoveExistingBookmarks(PdfDocument pdfDoc)
    {
        var catalog = pdfDoc.GetCatalog();
        var outlinesDict = catalog.GetPdfObject().GetAsDictionary(PdfName.Outlines);

        if (outlinesDict != null)
        {
            catalog.GetPdfObject().Remove(PdfName.Outlines);
            catalog.SetModified();
        }
    }
}