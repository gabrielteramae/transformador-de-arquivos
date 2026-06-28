namespace DataForge.Models;

public class TransformRequest
{
    public string? Filter { get; set; }
    public List<string>? SelectColumns { get; set; }
    public Dictionary<string, string>? RenameColumns { get; set; }
    public string OutputFormat { get; set; } = "json";
}

public class TransformResponse
{
    public bool Success { get; set; }
    public string? OutputFormat { get; set; }
    public int RowCount { get; set; }
    public string? Data { get; set; }
    public string? Error { get; set; }
}
