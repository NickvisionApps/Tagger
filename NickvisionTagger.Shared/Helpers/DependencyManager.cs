using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NickvisionTagger.Shared.Helpers;

internal static class DependencyManager
{
    private static string _fpcalcPath;

    static DependencyManager()
    {
        _fpcalcPath = "fpcalc";
    }

    /// <summary>
    /// The path for fpcalc
    /// </summary>
    public static string FpcalcPath
    {
        get
        {
            if(!File.Exists(_fpcalcPath))
            {
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    var prefixes = new List<string>() 
                    {
                        Directory.GetParent(Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!))!.FullName)!.FullName,
                        Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!))!.FullName,
                        "/usr"
                    };
                    foreach (var prefix in prefixes)
                    {
                        var path = $"{prefix}/bin/fpcalc";
                        if (File.Exists(path))
                        {
                            _fpcalcPath = path;
                            return _fpcalcPath;
                        }
                    }
                }
            }
            return _fpcalcPath;
        }
    }
}