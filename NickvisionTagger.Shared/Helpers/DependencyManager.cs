using Nickvision.Aura;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NickvisionTagger.Shared.Helpers;

internal static class DependencyManager
{
    /// <summary>
    /// The path for fpcalc
    /// </summary>
    public static string FpcalcPath
    {
        get
        {
            var fpcalc = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "fpcalc.exe" : "fpcalc";
            var assemblyPath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);
            if (File.Exists($"{assemblyPath}{Path.DirectorySeparatorChar}{fpcalc}"))
            {
                return $"{assemblyPath}{Path.DirectorySeparatorChar}{fpcalc}";
            }
            foreach (var dir in SystemDirectories.Path)
            {
                if (File.Exists($"{dir}{Path.DirectorySeparatorChar}{fpcalc}"))
                {
                    return $"{dir}{Path.DirectorySeparatorChar}{fpcalc}";
                }
            }
            throw new Exception("fpcalc not found");
        }
    }
}