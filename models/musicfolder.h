#ifndef MUSICFOLDER_H
#define MUSICFOLDER_H

#include <string>
#include <vector>
#include <memory>
#include "musicfile.h"

namespace NickvisionTagger::Models
{
    class MusicFolder
    {
    public:
        MusicFolder();
        const std::string& getPath() const;
        void setPath(const std::string& path);
        bool includeSubfolders() const;
        void setIncludeSubfolders(bool includeSubfolders);
        const std::vector<std::shared_ptr<MusicFile>>& getFiles() const;
        void reloadFiles();

    private:
        std::string m_path;
        bool m_includeSubfolders;
        std::vector<std::shared_ptr<MusicFile>> m_files;
    };
}

#endif // MUSICFOLDER_H
