#include "datamusicfilescolumns.h"

namespace NickvisionTagger::Models
{
    DataMusicFilesColumns::DataMusicFilesColumns()
    {
        add(m_colID);
        add(m_colFilename);
        add(m_colTitle);
        add(m_colArtist);
        add(m_colAlbum);
        add(m_colDuration);
        add(m_colComment);
        add(m_colPath);
    }

    const Gtk::TreeModelColumn<unsigned int>& DataMusicFilesColumns::getColID() const
    {
        return m_colID;
    }

    const Gtk::TreeModelColumn<std::string>& DataMusicFilesColumns::getColFilename() const
    {
        return m_colFilename;
    }

    const Gtk::TreeModelColumn<std::string>& DataMusicFilesColumns::getColTitle() const
    {
        return m_colTitle;
    }

    const Gtk::TreeModelColumn<std::string>& DataMusicFilesColumns::getColArtist() const
    {
        return m_colArtist;
    }

    const Gtk::TreeModelColumn<std::string>& DataMusicFilesColumns::getColAlbum() const
    {
        return m_colAlbum;
    }

    const Gtk::TreeModelColumn<std::string>& DataMusicFilesColumns::getColDuration() const
    {
        return m_colDuration;
    }

    const Gtk::TreeModelColumn<std::string>& DataMusicFilesColumns::getColComment() const
    {
        return m_colComment;
    }

    const Gtk::TreeModelColumn<std::string>& DataMusicFilesColumns::getColPath() const
    {
        return m_colPath;
    }
}
