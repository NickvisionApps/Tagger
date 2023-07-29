using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NickvisionTagger.GNOME.Helpers;

public static class Help
{
    public static string GetHelpURL(string pageName)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SNAP")))
        {
            return $"help:tagger/{pageName}";
        }
        using var linguasStream = Assembly.GetCallingAssembly().GetManifestResourceStream("NickvisionTagger.GNOME.LINGUAS");
        using var reader = new StreamReader(linguasStream!);
        var linguas = reader.ReadToEnd().Split(Environment.NewLine);
        var lang = "C";
        if (linguas.Contains(CultureInfo.CurrentCulture.Name.Replace("-", "_")))
        {
            lang = CultureInfo.CurrentCulture.Name.Replace("-", "_");
        }
        else
        {
            foreach (var l in linguas)
            {
                if (l.Contains(CultureInfo.CurrentCulture.TwoLetterISOLanguageName))
                {
                    lang = l;
                    break;
                }
            }
        }
        return $"https://htmlpreview.github.io/?https://raw.githubusercontent.com/NickvisionApps/Tagger/main/NickvisionTagger.Shared/Docs/html/{lang}/{pageName}.html";
    }
}