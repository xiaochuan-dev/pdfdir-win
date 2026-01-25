using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

public class BookChapterParser
{
    public static ObservableCollection<BookChapter> Parse(string text)
    {
        var chapters = new ObservableCollection<BookChapter>();
        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        // 存储每一行的缩进空格数
        List<int> indentLevels = new List<int>();

        // 第一遍：计算每行的缩进空格数
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                indentLevels.Add(-1); // 空行的标记
                continue;
            }

            int spaces = 0;
            foreach (char c in line)
            {
                if (c == ' ')
                {
                    spaces++;
                }
                else if (c == '\t')
                {
                    spaces += 4; // 假设制表符等于4个空格
                }
                else
                {
                    break;
                }
            }
            indentLevels.Add(spaces);
        }

        // 第二遍：建立树形结构
        var stack = new Stack<(BookChapter chapter, int spaces)>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;

            int currentSpaces = indentLevels[i];

            var (title, pageNumber) = ParseLine(trimmedLine);
            BookChapter chapter = new BookChapter
            {
                Title = title,
                PageNumber = pageNumber
            };

            if (stack.Count == 0)
            {
                // 第一行，添加到根节点
                chapters.Add(chapter);
                stack.Push((chapter, currentSpaces));
            }
            else
            {
                // 寻找合适的父节点
                var parentSpaces = stack.Peek().spaces;

                if (currentSpaces > parentSpaces)
                {
                    // 缩进增加，是子节点
                    var parent = stack.Peek().chapter;
                    parent.Children.Add(chapter);
                    stack.Push((chapter, currentSpaces));
                }
                else if (currentSpaces == parentSpaces)
                {
                    // 缩进相同，是兄弟节点
                    stack.Pop();
                    if (stack.Count > 0)
                    {
                        var newParent = stack.Peek().chapter;
                        newParent.Children.Add(chapter);
                    }
                    else
                    {
                        chapters.Add(chapter);
                    }
                    stack.Push((chapter, currentSpaces));
                }
                else
                {
                    // 缩进减少，需要向上回溯
                    while (stack.Count > 0 && stack.Peek().spaces >= currentSpaces)
                    {
                        stack.Pop();
                    }

                    if (stack.Count > 0)
                    {
                        var parent = stack.Peek().chapter;
                        parent.Children.Add(chapter);
                    }
                    else
                    {
                        chapters.Add(chapter);
                    }
                    stack.Push((chapter, currentSpaces));
                }
            }
        }

        return chapters;
    }

    private static (string title, int? pageNumber) ParseLine(string line)
    {
        var match = Regex.Match(line, @"^(.+?)(?:\s+(\d+))?$");

        if (match.Success)
        {
            string title = match.Groups[1].Value.TrimEnd();

            if (match.Groups[2].Success && int.TryParse(match.Groups[2].Value, out int page))
            {
                return (title, page);
            }
        }

        return (line, null);
    }
}