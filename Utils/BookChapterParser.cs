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
        var stack = new Stack<(BookChapter chapter, int level)>();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;

            int currentLevel = GetIndentLevel(line);

            var (title, pageNumber) = ParseLine(trimmedLine);

            BookChapter chapter = new BookChapter
            {
                Title = title,
                PageNumber = pageNumber
            };

            if (stack.Count == 0)
            {
                chapters.Add(chapter);
                stack.Push((chapter, currentLevel));
            }
            else
            {
                while (stack.Count > 0 && stack.Peek().level >= currentLevel)
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

                stack.Push((chapter, currentLevel));
            }
        }

        return chapters;
    }

    private static int GetIndentLevel(string line)
    {
        int level = 0;
        int spaceCount = 0;

        foreach (char c in line)
        {
            if (c == ' ')
            {
                spaceCount++;
                if (spaceCount == 4)
                {
                    level++;
                    spaceCount = 0;
                }
            }
            else if (c == '\t')
            {
                level++;
                spaceCount = 0;
            }
            else
            {
                break;
            }
        }

        if (spaceCount > 0)
        {
            level++;
        }

        return level;
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