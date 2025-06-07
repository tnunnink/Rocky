using System.ComponentModel;
using System.Text;
using L5Sharp.Core;
using Microsoft.SemanticKernel;

namespace Rocky.Cli.Plugins;

public class TagPlugin
{
    [KernelFunction]
    [Description("Gets all tags from an L5X file and returns their tag name")]
    public async Task<IEnumerable<string>> GetAllTags(string filePath)
    {
        var content = await L5X.LoadAsync(filePath, L5XOptions.Index);
        var tags = content.Query<Tag>();
        return tags.Select(t => t.TagName.ToString());
    }

    [KernelFunction, Description("Gets all tags of a specific data type from an L5X file")]
    public async Task<string> GetTagsByDataType(
        [Description("The full path to the L5X file")]
        string filePath,
        [Description("The data type to filter by (e.g., DINT, BOOL, REAL)")]
        string dataType)
    {
        try
        {
            var content = await L5X.LoadAsync(filePath, L5XOptions.Index);

            var tags = content.Query<Tag>()
                .Where(t => t.DataType.Equals(dataType, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var result = new StringBuilder();
            result.AppendLine($"Found {tags.Count} tags of type {dataType}:");

            foreach (var tag in tags)
            {
                result.AppendLine($"- {tag.Name}");
                if (!string.IsNullOrEmpty(tag.Description))
                    result.AppendLine($"  Description: {tag.Description}");
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error reading L5X file: {ex.Message}";
        }
    }
}