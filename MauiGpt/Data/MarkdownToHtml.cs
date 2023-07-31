using System.Text;
using System.Text.RegularExpressions;

namespace MauiGpt.Data;


public class MarkdownToHtml
{
    private static readonly Regex _mdCodeRegex = new Regex(@"```(\w+)?");
    public const string MdText = "md";
    private const string CodeText = "code";


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