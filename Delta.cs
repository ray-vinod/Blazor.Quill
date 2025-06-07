using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blazor.Quill;
public static class DeltaHandler
{
    public static Delta ToDelta(this string json)
    {
        var deltaJson = JsonSerializer.Deserialize<Delta>(json);
        List<Operation> jsonOps = deltaJson?.Ops ?? [];
        List<Operation> ops = [];

        foreach (var op in jsonOps)
        {
            if (op.Insert is JsonElement insertElement)
            {
                switch (insertElement.ValueKind)
                {
                    case JsonValueKind.String:
                        ops.Add(new() { Insert = insertElement.GetString()!, Attributes = op.Attributes });
                        break;
                    case JsonValueKind.Object:
                        if (insertElement.TryGetProperty("image", out var imageElement))
                        {
                            var image = JsonSerializer.Deserialize<ImageFile>(imageElement.GetRawText());
                            ops.Add(new() { Insert = image!, Attributes = op.Attributes });
                        }
                        break;
                }
            }
        }

        return new Delta(ops);
    }
}

public class Delta(List<Operation> ops)
{
    [JsonPropertyName("ops")]
    public List<Operation> Ops { get; set; } = ops;
}

public class Operation
{
    [JsonPropertyName("insert")]
    public object Insert { get; set; } = new();

    [JsonPropertyName("attributes")]
    public Dictionary<string, object> Attributes { get; set; } = [];
}

public class ImageFile
{
    [JsonPropertyName("src")]
    public string Src { get; set; } = string.Empty;

    [JsonPropertyName("alt")]
    public string Alt { get; set; } = string.Empty;
}