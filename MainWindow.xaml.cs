using System;
using System.Windows;
using System.IO;
using Microsoft.Win32;

namespace pdfdir_win
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel => DataContext as MainViewModel;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }
        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "选择文件";
            openFileDialog.Filter = "所有文件 (*.*)|*.*|文本文件 (*.txt)|*.txt|PDF文件 (*.pdf)|*.pdf";
            openFileDialog.FilterIndex = 3;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                ViewModel.CurrentFile = new FileInfo(openFileDialog.SafeFileName, openFileDialog.FileName);
            }
        }

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            string text = InputTextBox.Text;
            try
            {
                var res = BookChapterParser.Parse(text);
                if (res.Count != 0)
                {
                    var newBookViewModel = new BookViewModel(res);
                    ViewModel.BookDirectory = newBookViewModel;

                    var filePath = ViewModel.CurrentFile.Path;
                    var directory = Path.GetDirectoryName(filePath);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                    var newFileName = $"{fileNameWithoutExtension}_new.pdf";
                    var newFilePath = Path.Combine(directory, newFileName);

                    var offset = NumberInput.Value ?? 0;
                    var isOverwrite = EnableCheckBox.IsChecked ?? false;

                    if (isOverwrite)
                    {
                        BookChapterExporter.WriteChaptersToPdf(ViewModel.CurrentFile.Path, res, offset);
                    }
                    else
                    {
                        BookChapterExporter.WriteChaptersToPdf(ViewModel.CurrentFile.Path, newFilePath, res, offset);
                    }
                    MessageBox.Show("写入成功");
                }
                else
                {
                    MessageBox.Show("提取目录失败，请修改重试");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("写入失败，请修改重试");
            }
        }
    }
}