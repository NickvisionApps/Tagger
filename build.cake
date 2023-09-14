const string appId = "org.nickvision.tagger";
const string projectName = "NickvisionTagger";
const string shortName = "tagger";
readonly string[] projectsToBuild = new string[] { "GNOME" };

if (FileExists("CakeScripts/main.cake"))
{
    #load local:?path=CakeScripts/main.cake
}
else
{
    throw new CakeException("Failed to load main script.");
}