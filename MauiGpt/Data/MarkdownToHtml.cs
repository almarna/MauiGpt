using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using MauiGpt.Dto;

namespace MauiGpt.Data;


public class MarkdownToHtml
{
    private static readonly Regex _mdCodeRegex = new Regex(@"```(\w+)?");
    public const string MdText = "md";
    public const string CodeText = "code";

    public async Task<IList<HtmlBlock>> Convert(string indata, Func<string, string, Task<string>> convertCode)
    {
        var blocks = GetBlocks(indata);

        var result = new List<HtmlBlock>();

        foreach (var (type, content) in blocks)
        {
            if (type == MarkdownToHtml.MdText)
            {
                var html = "";
                try
                {
                    html = Markdown.ToHtml(content);
                }
                catch (Exception e) // Fixlösning för att ibland klarar inte Markdig fragment, men funkar om man omringar det med t.ex. en span
                {
                    html = Markdown.ToHtml($"<code>{content}</code>");
                }

                result.Add(new HtmlBlock { Type = BlockType.Plain, Html = html });
            }
            else
            {
                string colored = await convertCode(type == CodeText ? "" : type, content);
                result.Add(new HtmlBlock { Type = BlockType.Code, Html = colored });
            }
        }

        return result;
    }


    public List<(string type, string content)> GetBlocks(string indata)
    {
        MatchCollection matches = _mdCodeRegex.Matches(indata);

        bool inCodeblock = false;
        int lastPosition = 0;
        string blockType = MdText;

        List<(string type, string content)> blocks = new List<(string, string)>();

        foreach (Match match in matches)
        {
            string content = indata.Substring(lastPosition, match.Index - lastPosition);

            blocks.Add((blockType, content));

            inCodeblock = !inCodeblock;

            blockType = GetBlockType(inCodeblock, match.Groups[1].Value);

            lastPosition = match.Index + match.Length;
        }

        string lastContent = indata.Substring(lastPosition, indata.Length - lastPosition);

        blocks.Add((blockType, lastContent));
        return blocks;
    }

    private static string GetBlockType(bool inCodeblock, string value)
    {
        if (inCodeblock)
        {
            return string.IsNullOrWhiteSpace(value) ? CodeText : value;
        }

        return MdText;
    }
}