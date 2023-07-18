using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NickvisionTagger.Shared.Helpers;

internal static class DependencyManager
{
    private static string _fpcalcPath;
    private static string _ffprobePath;

    static DependencyManager()
    {
        _fpcalcPath = "";
        _ffprobePath = "";
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
                var prefixes = new List<string>() {
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
                _fpcalcPath = "fpcalc";
            }
            return _fpcalcPath;
        }
    }

    /// <summary>
    /// The path for ffprove
    /// </summary>
    public static string FfprobePath
    {
        get
        {
            if(!File.Exists(_ffprobePath))
            {
                var prefixes = new List<string>() {
                    Directory.GetParent(Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!))!.FullName)!.FullName,
                    Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!))!.FullName,
                    "/usr"
                };
                foreach (var prefix in prefixes)
                {
                    var path = $"{prefix}/bin/ffprobe";
                    if (File.Exists(path))
                    {
                        _ffprobePath = path;
                        return _ffprobePath;
                    }
                }
                _ffprobePath = "ffprobe";
            }
            return _ffprobePath;
        }
    }
}