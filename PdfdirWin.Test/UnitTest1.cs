using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlaUI.Core;
using FlaUI.UIA3;
using FlaUI.Core.AutomationElements;
using System;
using System.IO;
using System.Threading;

namespace PdfdirWin.UITests
{
    [TestClass]
    public class PdfdirWinUITests
    {
        private Application _app;
        private UIA3Automation _automation;

        [TestInitialize]
        public void TestInitialize()
        {
            // 获取应用路径（根据你的项目结构调整）
            var projectDir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent;
            var appPath = Path.Combine(projectDir.FullName, "PdfdirWin", "bin", "Debug", "net8.0-windows", "PdfdirWin.exe");
            
            if (!File.Exists(appPath))
            {
                // 尝试 Release 版本
                appPath = Path.Combine(projectDir.FullName, "PdfdirWin", "bin", "Release", "net8.0-windows", "PdfdirWin.exe");
            }
            
            Assert.IsTrue(File.Exists(appPath), $"应用未找到: {appPath}");
            
            // 启动应用
            _app = Application.Launch(appPath);
            _automation = new UIA3Automation();
            
            // 等待应用启动
            Thread.Sleep(2000);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                // 关闭应用
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
        public void MainWindow_ShouldOpen()
        {
            // 获取主窗口
            var mainWindow = _app.GetMainWindow(_automation);
            
            // 断言
            Assert.IsNotNull(mainWindow, "主窗口不能为 null");
            Assert.IsTrue(mainWindow.IsEnabled, "主窗口应启用");
            Assert.IsFalse(string.IsNullOrEmpty(mainWindow.Title), "主窗口标题不应为空");
            
            Console.WriteLine($"窗口标题: {mainWindow.Title}");
            Console.WriteLine($"窗口位置: {mainWindow.BoundingRectangle}");
        }

        [TestMethod]
        [TestCategory("UI")]
        public void Button_Click_ShouldWork()
        {
            var mainWindow = _app.GetMainWindow(_automation);
            
            // 查找按钮（根据你的实际控件名称）
            var buttons = mainWindow.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button));
            
            Console.WriteLine($"找到 {buttons.Length} 个按钮");
            
            foreach (var button in buttons)
            {
                var btn = button.AsButton();
                Console.WriteLine($"按钮: 名称={btn.Name}, ID={btn.Properties.AutomationId}, 文本={btn.AsLabel().Text}");
            }
            
            // 尝试点击第一个按钮（如果有）
            if (buttons.Length > 0)
            {
                var firstButton = buttons[0].AsButton();
                Console.WriteLine($"点击按钮: {firstButton.Name}");
                firstButton.Invoke();
                
                // 等待响应
                Thread.Sleep(1000);
            }
        }
    }
}