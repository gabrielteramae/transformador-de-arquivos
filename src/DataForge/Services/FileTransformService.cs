using System.Globalization;
using System.Text;
using System.Xml.Linq;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using DataForge.Models;
using Newtonsoft.Json;

namespace DataForge.Services;

public class FileTransformService : IFileTransformService
{
    private static readonly HashSet<string> AllowedExtensions = [".csv", ".json", ".xml", ".xlsx"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public async Task<TransformResponse> TransformAsync(Stream fileStream, string fileName, TransformRequest request)
    {
        try
        {
            var extension = Path.GetExtension(fileName).ToLower();

            if (!AllowedExtensions.Contains(extension))
                return new TransformResponse
                {
                    Success = false,
                    Error = $"Tipo de arquivo não suportado: '{extension}'. Use .csv, .json, .xml ou .xlsx."
                };

            if (fileStream.CanSeek && fileStream.Length > MaxFileSizeBytes)
                return new TransformResponse
                {
                    Success = false,
                    Error = "Arquivo muito grande. O limite é 5 MB."
                };

            List<Dictionary<string, string>> rows = extension switch
            {
                ".csv" => await ParseCsvAsync(fileStream),
                ".json" => await ParseJsonAsync(fileStream),
                ".xml" => await ParseXmlAsync(fileStream),
                ".xlsx" => await ParseXlsxAsync(fileStream),
                _ => []
            };

            rows = ApplyFilter(rows, request.Filter);
            rows = ApplySelectColumns(rows, request.SelectColumns);
            rows = ApplyRenameColumns(rows, request.RenameColumns);

            var output = request.OutputFormat.ToLower() switch
            {
                "csv" => SerializeToCsv(rows),
                "xml" => SerializeToXml(rows),
                _ => SerializeToJson(rows)
            };

            return new TransformResponse
            {
                Success = true,
                OutputFormat = request.OutputFormat,
                RowCount = rows.Count,
                Data = output
            };
        }
        catch (Exception ex)
        {
            return new TransformResponse { Success = false, Error = ex.Message };
        }
    }

    private static async Task<List<Dictionary<string, string>>> ParseCsvAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));

        var records = new List<Dictionary<string, string>>();
        await csv.ReadAsync();
        csv.ReadHeader();
        var headers = csv.HeaderRecord!;

        while (await csv.ReadAsync())
        {
            var row = new Dictionary<string, string>();
            foreach (var header in headers)
                row[header] = csv.GetField(header) ?? "";
            records.Add(row);
        }

        return records;
    }

    private static async Task<List<Dictionary<string, string>>> ParseJsonAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();
        var parsed = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(content);
        return parsed ?? throw new InvalidDataException("JSON inválido. O arquivo deve conter um array de objetos.");
    }

    private static Task<List<Dictionary<string, string>>> ParseXmlAsync(Stream stream)
    {
        var doc = XDocument.Load(stream);
        var rows = doc.Root?.Elements()
            .Select(el => el.Elements()
                .ToDictionary(e => e.Name.LocalName, e => e.Value))
            .ToList() ?? [];

        return Task.FromResult(rows);
    }

    private static Task<List<Dictionary<string, string>>> ParseXlsxAsync(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheets.First();
        var rows = new List<Dictionary<string, string>>();

        var headerRow = sheet.Row(1);
        var headers = headerRow.CellsUsed()
            .Select(c => c.GetString().Trim())
            .ToList();

        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < headers.Count; i++)
                dict[headers[i]] = row.Cell(i + 1).GetString();
            rows.Add(dict);
        }

        return Task.FromResult(rows);
    }

    private static List<Dictionary<string, string>> ApplyFilter(
        List<Dictionary<string, string>> rows, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return rows;

        var ops = new[] { "!=", ">=", "<=", ">", "<", "=" };
        foreach (var op in ops)
        {
            var idx = filter.IndexOf(op);
            if (idx < 0) continue;

            var field = filter[..idx].Trim();
            var value = filter[(idx + op.Length)..].Trim();

            return rows.Where(row =>
            {
                if (!row.TryGetValue(field, out var cellValue)) return false;

                if (double.TryParse(cellValue, out var numCell) &&
                    double.TryParse(value, out var numVal))
                {
                    return op switch
                    {
                        ">" => numCell > numVal,
                        "<" => numCell < numVal,
                        ">=" => numCell >= numVal,
                        "<=" => numCell <= numVal,
                        "!=" => numCell != numVal,
                        "=" => numCell == numVal,
                        _ => false
                    };
                }

                return op switch
                {
                    "=" => cellValue == value,
                    "!=" => cellValue != value,
                    _ => false
                };
            }).ToList();
        }

        return rows;
    }

    private static List<Dictionary<string, string>> ApplySelectColumns(
        List<Dictionary<string, string>> rows, List<string>? columns)
    {
        if (columns == null || columns.Count == 0) return rows;
        return rows.Select(row =>
            columns.ToDictionary(col => col, col => row.GetValueOrDefault(col, ""))
        ).ToList();
    }

    private static List<Dictionary<string, string>> ApplyRenameColumns(
        List<Dictionary<string, string>> rows, Dictionary<string, string>? renames)
    {
        if (renames == null || renames.Count == 0) return rows;
        return rows.Select(row =>
            row.ToDictionary(
                kvp => renames.GetValueOrDefault(kvp.Key, kvp.Key),
                kvp => kvp.Value)
        ).ToList();
    }

    private static string SerializeToJson(List<Dictionary<string, string>> rows) =>
        JsonConvert.SerializeObject(rows, Formatting.Indented);

    private static string SerializeToCsv(List<Dictionary<string, string>> rows)
    {
        if (rows.Count == 0) return "";
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", rows[0].Keys));
        foreach (var row in rows)
            sb.AppendLine(string.Join(",", row.Values.Select(v => $"\"{v}\"")));
        return sb.ToString();
    }

    private static string SerializeToXml(List<Dictionary<string, string>> rows)
    {
        var root = new XElement("Records",
            rows.Select(row =>
                new XElement("Record",
                    row.Select(kvp => new XElement(kvp.Key, kvp.Value)))));
        return root.ToString();
    }
}