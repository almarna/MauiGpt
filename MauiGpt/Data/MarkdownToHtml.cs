using System.Text;
using System.Text.RegularExpressions;

namespace MauiGpt.Data;

using Markdig;
using Microsoft.JSInterop;

public class MarkdownToHtml
{
    private readonly IJSRuntime _js;
    // private readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseColorCode().Build();
    //const string codePattern = @"`{3}(?<lang>[a-zA-Z]+)\n";
    //private const string cp2 = @"```(\w+)";

    private static readonly Regex _mdCodeRegex = new Regex(@"```(\w+)?");
    public const string MdText = "md";
    private const string CodeText = "code";

    private string sample = @"Here's an example of an inner loop in C#:

```csharp
//initialize the outer and inner loop counters
for (int i = 0; i < 5; i++)
{
    Console.WriteLine(""Outer loop counter: "" + i);

    for (int j = 0; j < 3; j++)
    {
        Console.WriteLine(""Inner loop counter: "" + j);
    }
}
```

In this example, the outer loop runs five times, while the inner loop runs three times for each iteration of the outer loop. The output of this code would be:

```
Outer loop counter: 0
Inner loop counter: 0
Inner loop counter: 1
Inner loop counter: 2
Outer loop counter: 1
Inner loop counter: 0
Inner loop counter: 1
Inner loop counter: 2
Outer loop counter: 2
Inner loop counter: 0
Inner loop counter: 1
Inner loop counter: 2
Outer loop counter: 3
Inner loop counter: 0
Inner loop counter: 1
Inner loop counter: 2
Outer loop counter: 4
Inner loop counter: 0
Inner loop counter: 1
Inner loop counter: 2
```

As you can see, the inner loop runs through all three iterations for each of the five iterations of the outer loop.
";


    public MarkdownToHtml(IJSRuntime jsRuntime)
    {
        _js = jsRuntime;
//        Convert(sample);
    }

    public async Task<string> Convert(string indata)
    {
        var blocks = GetBlocks(indata);

        StringBuilder builder = new StringBuilder();

        foreach (var block in blocks)
        {
            if (block.type == MdText)
            {
                builder.Append(block.content);
            }
            else
            {
                string colored = await ColorCodeblock(block.content);
                builder.Append(colored);
            }
        }

        string result = Markdown.ToHtml(builder.ToString());

        Console.WriteLine(blocks.Count);

        return Markdown.ToHtml(indata);
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

    private async Task<string> ColorCodeblock(string uncolored)
    {
        return await _js.InvokeAsync<string>("colorCodeblock", uncolored);

    }
}