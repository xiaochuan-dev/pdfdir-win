using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace PdfdirWin.UITests
{
    public static class TestLogger
    {
        private static string _logFilePath;
        private static readonly object _lock = new object();
        private static bool _isInitialized = false;
        private static string _projectRootPath;

        /// <summary>
        /// 获取项目根目录
        /// </summary>
        public static string GetProjectRootPath()
        {
            if (_projectRootPath != null)
                return _projectRootPath;

            // 方法1：尝试获取解决方案目录
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDir = Path.GetDirectoryName(assemblyLocation);

            // 向上查找直到找到 .csproj 或 .sln 文件
            var currentDir = assemblyDir;
            while (currentDir != null)
            {
                if (Directory.GetFiles(currentDir, "*.csproj").Length > 0 ||
                    Directory.GetFiles(currentDir, "*.sln").Length > 0)
                {
                    _projectRootPath = currentDir;
                    return _projectRootPath;
                }
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            // 方法2：如果找不到，使用当前目录
            _projectRootPath = Directory.GetCurrentDirectory();
            return _projectRootPath;
        }

        /// <summary>
        /// 初始化日志系统
        /// </summary>
        public static void Initialize(string logFileName = null, bool append = false)
        {
            lock (_lock)
            {
                if (string.IsNullOrEmpty(logFileName))
                {
                    logFileName = $"TestLog_{DateTime.Now:yyyyMMdd_HHmmss}.log";
                }

                string projectRoot = GetProjectRootPath();
                string logDirectory = Path.Combine(projectRoot, "TestLogs");

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                _logFilePath = Path.Combine(logDirectory, logFileName);

                if (!append && File.Exists(_logFilePath))
                {
                    File.Delete(_logFilePath);
                }

                _isInitialized = true;

                // 写入初始化信息
                WriteLine($"=== 测试日志系统初始化 ===");
                WriteLine($"日志文件: {_logFilePath}");
                WriteLine($"初始化时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                WriteLine($"=======================================\n");
            }
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        public static void Write(string message, LogLevel level = LogLevel.Info)
        {
            EnsureInitialized();

            lock (_lock)
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";

                // 写入文件
                File.AppendAllText(_logFilePath, logEntry, System.Text.Encoding.UTF8);

                // 同时输出到调试窗口
                Debug.WriteLine(logEntry);

                // 同时输出到控制台（确保能看到）
                Console.WriteLine($"[TEST-LOG] {logEntry.Trim()}");
            }
        }

        /// <summary>
        /// 写入日志并换行
        /// </summary>
        public static void WriteLine(string message = "", LogLevel level = LogLevel.Info)
        {
            Write(message + Environment.NewLine, level);
        }

        /// <summary>
        /// 写入分隔线
        /// </summary>
        public static void WriteSeparator(string title = "")
        {
            string separator = string.IsNullOrEmpty(title)
                ? "======================================="
                : $"========== {title} ==========";

            WriteLine(separator, LogLevel.Info);
        }

        /// <summary>
        /// 记录测试开始
        /// </summary>
        public static void StartTest(string testName, string description = "")
        {
            WriteLine();
            WriteLine();
            WriteSeparator($"测试开始: {testName}");
            WriteLine($"测试名称: {testName}", LogLevel.Info);

            if (!string.IsNullOrEmpty(description))
            {
                WriteLine($"测试描述: {description}", LogLevel.Info);
            }

            WriteLine($"开始时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}", LogLevel.Info);
            WriteLine();
        }

        /// <summary>
        /// 记录测试结束
        /// </summary>
        public static void EndTest(string testName, bool passed = true, string message = "")
        {
            WriteLine();
            WriteLine($"结束时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}", LogLevel.Info);
            WriteLine($"测试结果: {(passed ? "✓ 通过" : "✗ 失败")}",
                      passed ? LogLevel.Success : LogLevel.Error);

            if (!string.IsNullOrEmpty(message))
            {
                WriteLine($"结果信息: {message}",
                          passed ? LogLevel.Info : LogLevel.Error);
            }

            WriteSeparator($"测试结束: {testName}");
            WriteLine();
        }

        /// <summary>
        /// 记录异常信息
        /// </summary>
        public static void LogException(Exception ex, string context = "")
        {
            WriteLine();
            WriteSeparator("异常发生");

            if (!string.IsNullOrEmpty(context))
            {
                WriteLine($"上下文: {context}", LogLevel.Error);
            }

            WriteLine($"异常类型: {ex.GetType().Name}", LogLevel.Error);
            WriteLine($"异常消息: {ex.Message}", LogLevel.Error);
            WriteLine($"堆栈跟踪:", LogLevel.Error);
            WriteLine(ex.StackTrace, LogLevel.Error);

            // 递归记录内部异常
            Exception inner = ex.InnerException;
            int level = 1;
            while (inner != null)
            {
                WriteLine();
                WriteLine($"内部异常 #{level}:", LogLevel.Error);
                WriteLine($"类型: {inner.GetType().Name}", LogLevel.Error);
                WriteLine($"消息: {inner.Message}", LogLevel.Error);

                inner = inner.InnerException;
                level++;
            }

            WriteSeparator("异常结束");
            WriteLine();
        }

        /// <summary>
        /// 获取日志文件路径
        /// </summary>
        public static string GetLogFilePath()
        {
            EnsureInitialized();

            // 同时在控制台输出路径，方便查找
            Console.WriteLine($"\n[TEST-INFO] 日志文件路径: {_logFilePath}");
            Console.WriteLine($"[TEST-INFO] 文件是否存在: {File.Exists(_logFilePath)}");

            return _logFilePath;
        }

        /// <summary>
        /// 打开日志文件
        /// </summary>
        public static void OpenLogFile()
        {
            string filePath = GetLogFilePath();

            try
            {
                if (File.Exists(filePath))
                {
                    Console.WriteLine($"[TEST-INFO] 正在打开日志文件: {filePath}");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    Console.WriteLine($"[TEST-ERROR] 日志文件不存在: {filePath}");
                    Console.WriteLine($"[TEST-ERROR] 当前目录: {Directory.GetCurrentDirectory()}");

                    // 列出当前目录内容
                    Console.WriteLine($"[TEST-INFO] 当前目录内容:");
                    foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
                    {
                        Console.WriteLine($"  {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TEST-ERROR] 无法打开日志文件: {ex.Message}");
            }
        }

        /// <summary>
        /// 确保已初始化
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }
    }

    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Success,
        Warning,
        Error
    }
}