using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlaUI.Core;
using FlaUI.UIA3;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using System.IO;
using System.Diagnostics;
using Application = FlaUI.Core.Application;
using FlaUI.Core.Input;

namespace PdfdirWin.UITests;
[TestClass]
public class PdfdirWinUITests
{
    private Application _app;
    private UIA3Automation _automation;

    [TestInitialize]
    public void TestInitialize()
    {
        var projectDir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent;
        var appPath = Path.Combine(projectDir.FullName, "PdfdirWin", "bin", "Debug", "net8.0-windows", "PdfdirWin.exe");

        if (!File.Exists(appPath))
        {
            appPath = Path.Combine(projectDir.FullName, "PdfdirWin", "bin", "Release", "net8.0-windows", "PdfdirWin.exe");
        }

        Assert.IsTrue(File.Exists(appPath), $"应用未找到: {appPath}");

        _app = Application.Launch(appPath);
        _automation = new UIA3Automation();

        Thread.Sleep(2000);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        try
        {
            _app?.Close();
            _app?.Dispose();
            _automation?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"清理时出错: {ex.Message}");
        }
    }

    [TestMethod]
    [TestCategory("UI")]
    public void SetFilePathDirectly()
    {
        var mainWindow = _app.GetMainWindow(_automation);

        var chooseFileBtn = mainWindow.FindFirstDescendant(
            cf => cf.ByName("选择文件")
        )?.AsButton();

        if (chooseFileBtn != null)
        {
            TestLogger.WriteLine($"找到按钮: {chooseFileBtn.Name}");
            chooseFileBtn.Click();
        }
        else
        {
            TestLogger.WriteLine("未找到指定文本的按钮");
        }

        Thread.Sleep(1000);

        var dialog = _automation.GetDesktop()
            .FindFirstDescendant(cf => cf.ByClassName("#32770"))?.AsWindow();

        if (dialog == null)
        {
            TestLogger.WriteLine("未找到文件对话框");
            return;
        }

        var fileNameComboBox = dialog.FindFirstDescendant(cf => cf.ByAutomationId("1148"))?.AsComboBox();
        if (fileNameComboBox == null)
        {
            TestLogger.WriteLine("未找到文件名组合框");
            return;
        }

        var fileNameTextBox = fileNameComboBox.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit))?.AsTextBox();
        if (fileNameTextBox == null)
        {
            TestLogger.WriteLine("未找到文件名输入框");
            return;
        }

        string fullPath = Environment.GetEnvironmentVariable("MY_TEST_FILEPATH");
        string fileName = Path.GetFileName(fullPath);

        fileNameTextBox.Text = fileName;

        if (fileNameTextBox.Patterns.Value.IsSupported)
        {
            fileNameTextBox.Patterns.Value.Pattern.SetValue(fileName);
        }
        else
        {
            fileNameTextBox.Text = fileName;
        }

        var openButton = dialog.FindFirstDescendant(cf =>
            cf.ByControlType(ControlType.Button).And(cf.ByAutomationId("1")))?.AsButton();

        TestLogger.WriteLine($"打开按钮 {openButton.Name}");

        openButton?.Invoke();
    
        Thread.Sleep(1000);

        TestLogger.WriteLine("文件选择完成");
        var inputTextBox = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("InputTextBox"))?.AsTextBox();
        string baseDirectory = TestLogger.GetProjectRootPath();
        string filePath = Environment.GetEnvironmentVariable("MY_TEST_DATAPATH");
        var textContent = File.ReadAllText(filePath);

        if (inputTextBox != null)
        {
            inputTextBox.Text = textContent;
        }
        else
        {
            Assert.Fail("未找到 InputTextBox");
        }

        Thread.Sleep(2000);
        var writeBtn = mainWindow.FindFirstDescendant(
            cf => cf.ByName("写入")
        )?.AsButton();

        var stopwatch = Stopwatch.StartNew();
        writeBtn?.Click();

        var checkCount = 0;
        
        var interval = TimeSpan.FromMilliseconds(10);
        var timeout = TimeSpan.FromSeconds(5);
        
        while (stopwatch.Elapsed < timeout)
        {
            checkCount++;
            var messageBox = _automation.GetDesktop()
                .FindFirstDescendant(cf => cf.ByClassName("#32770"))?.AsWindow();
            
            if (messageBox != null)
            {
                stopwatch.Stop();
                var firstButton = messageBox.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button));
                if (firstButton != null)
                {
                    firstButton.AsButton().Invoke();
                    TestLogger.WriteLine("已点击第一个按钮关闭弹窗");
                }
                else
                {
                    messageBox.Close();
                    TestLogger.WriteLine("直接关闭消息框");
                }
                
                TestLogger.WriteLine($"找到 MessageBox，耗时: {stopwatch.ElapsedMilliseconds}ms");
                TestLogger.WriteLine($"检查次数: {checkCount}");
                TestLogger.WriteLine($"平均每次检查耗时: {(double)stopwatch.ElapsedMilliseconds / checkCount:F2}ms");
                return;
            }
            
            Thread.Sleep(interval);
        }
        
        stopwatch.Stop();
        TestLogger.WriteLine($"未找到 MessageBox，总耗时: {stopwatch.ElapsedMilliseconds}ms");
    }
}