using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

public class FfprobeFormat
{
    public string Filename { get; set; }
    public Dictionary<string, string> Tags { get; set; }

    public FfprobeFormat()
    {
        Filename = "";
        Tags = new Dictionary<string, string>();
    }
    
    public override string ToString()
    {
        var s = $"==={Filename}===\n";
        foreach (var pair in Tags)
        {
            s += $"{pair.Key} - {pair.Value}\n";
        }
        s = s.Remove(s.Length - 1, 1);
        return s;
    }
}

public static class FfprobeHelpers
{
    public static FfprobeFormat? GetFormat(string ffprobePath, string filePath)
    {
        using var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = ffprobePath,
                Arguments = $"\"{filePath}\" -print_format json -v quiet -show_format",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if(process.ExitCode != 0)
        {
            return null;
        }
        var options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };
        output = output.Remove(output.IndexOf("\"format\""), 11);
        output = output.Remove(output.Length - 2, 2);
        return JsonSerializer.Deserialize<FfprobeFormat>(output, options);
    }
}