using System.Text;
using System.Text.Json;
using RapidOCRSharpOnnx;
using RapidOCRSharpOnnx.Configurations;
using RapidOCRSharpOnnx.Models;
using RapidOCRSharpOnnx.Providers;
using RapidOCRSharpOnnx.Utils;

// Force UTF-8 output so Pot receives Chinese characters correctly
Console.OutputEncoding = Encoding.UTF8;

// Usage: pot_ocr.exe <image_path> [tiny|small|medium]
var imagePath = args.Length > 0 ? args[0] : null;
var modelSize = args.Length > 1 ? args[1].ToLower() : "small";

if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
{
    Console.Error.WriteLine("Usage: PPOCRv6.exe <image_path> [tiny|small|medium]");
    Console.Error.WriteLine("  Default model size: small");
    Environment.Exit(1);
}

if (modelSize is not ("tiny" or "small" or "medium"))
{
    Console.Error.WriteLine($"Invalid model size: {modelSize}. Use tiny, small, or medium.");
    Environment.Exit(1);
}

// Robust model path resolution
static string FindModel(string fileName)
{
    var searchDirs = new[] {
        AppContext.BaseDirectory,
        Path.Combine(AppContext.BaseDirectory, "..", "..", ".."),
        Environment.CurrentDirectory
    };
    foreach (var dir in searchDirs)
    {
        var full = Path.GetFullPath(Path.Combine(dir, "models", fileName));
        if (File.Exists(full)) return full;
    }
    return Path.Combine(AppContext.BaseDirectory, "models", fileName);
}

var detPath = FindModel($"ch_PP-OCRv6_{modelSize}_det.onnx");
var recPath = FindModel($"ch_PP-OCRv6_{modelSize}_rec.onnx");
var clsPath = FindModel("ch_PP-LCNet_x0_25_textline_ori_cls.onnx");

var ocrVersion = modelSize == "tiny" ? OCRVersion.PPOCRV6Tiny : OCRVersion.PPOCRV6;

try
{
    using var ocr = new RapidOCRSharp(
        new ExecutionProviderCPU(
            new OcrConfig(detPath, recPath, LangRec.CH, ocrVersion, clsPath)
        )
    );

    var result = ocr.RecognizeText(imagePath);

    // Build JSON output
    var texts = new List<object>();
    if (result.DetResult?.Data?.DetItems != null)
    {
        var recResults = result.RecResult?.Data ?? Array.Empty<RecResult>();
        var detItems = result.DetResult.Data.DetItems;

        for (int i = 0; i < detItems.Length; i++)
        {
            var detItem = detItems[i];
            var rec = i < recResults.Length ? recResults[i] : null;

            texts.Add(new
            {
                text = rec?.Label ?? "",
                score = rec?.Score ?? 0f,
                box = detItem.Box?.Select(p => new { x = (int)p.X, y = (int)p.Y }).ToArray()
            });
        }
    }
    else
    {
        texts.Add(new { text = result.TextBlocks ?? "", score = 1.0f, box = Array.Empty<object>() });
    }

    var output = JsonSerializer.Serialize(new { code = 100, data = texts },
        new JsonSerializerOptions { WriteIndented = false, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

    Console.WriteLine(output);
}
catch (Exception ex)
{
    var error = JsonSerializer.Serialize(new { code = 101, error = ex.Message },
        new JsonSerializerOptions { WriteIndented = false });
    Console.Error.WriteLine(error);
    Environment.Exit(1);
}
