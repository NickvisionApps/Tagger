using Nickvision.Avalonia.MVVM;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NickvisionTagger.Models;

public class MusicFolder : ObservableObject
{
    private string _path;
    private ObservableCollection<MusicFile>? _files;

    public bool IncludeSubfolders { get; set; }

    public bool IsOpen => Path != "No Folder Open";

    public MusicFolder()
    {
        _path = "No Folder Open";
        _files = null;
        IncludeSubfolders = true;
    }

    public string Path
    {
        get => _path;

        set
        {
            SetProperty(ref _path, value);
            OnPropertyChanged("IsOpen");
        }
    }

    public ObservableCollection<MusicFile>? Files
    {
        get => _files;

        private set => SetProperty(ref _files, value);
    }

    public async Task ReloadFilesAsync()
    {
        Files = null;
        if (Directory.Exists(Path))
        {
            var files = new List<MusicFile>();
            var searchOption = IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var extensions = new string[] { ".mp3", ".wav", ".wma", ".flac", ".ogg" };
            await Task.Run(() =>
            {
                foreach (var path in Directory.EnumerateFiles(Path, "*.*", searchOption).Where(x => extensions.Contains(System.IO.Path.GetExtension(x).ToLower())))
                {
                    files.Add(new MusicFile(path));
                }
                files.Sort();
                Files = new ObservableCollection<MusicFile>(files);
                for(int i = 0; i < files.Count; i++)
                {
                    var musicFile = files[i];
                    musicFile.Id = i + 1;
                }
            });
        }
    }

    public void CloseFolder()
    {
        Path = "No Folder Open";
        Files = null;
    }
}
