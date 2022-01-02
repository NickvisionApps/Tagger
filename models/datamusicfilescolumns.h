#ifndef DATAMUSICFILESCOLUMNS_H
#define DATAMUSICFILESCOLUMNS_H

#include <string>
#include <gtkmm.h>

namespace NickvisionTagger::Models
{
    class DataMusicFilesColumns : public Gtk::TreeModel::ColumnRecord
    {
    public:
        DataMusicFilesColumns();
        const Gtk::TreeModelColumn<unsigned int>& getColID() const;
        const Gtk::TreeModelColumn<std::string>& getColFilename() const;
        const Gtk::TreeModelColumn<std::string>& getColTitle() const;
        const Gtk::TreeModelColumn<std::string>& getColArtist() const;
        const Gtk::TreeModelColumn<std::string>& getColAlbum() const;
        const Gtk::TreeModelColumn<std::string>& getColDuration() const;
        const Gtk::TreeModelColumn<std::string>& getColComment() const;
        const Gtk::TreeModelColumn<std::string>& getColPath() const;

    private:
        Gtk::TreeModelColumn<unsigned int> m_colID;
        Gtk::TreeModelColumn<std::string> m_colFilename;
        Gtk::TreeModelColumn<std::string> m_colTitle;
        Gtk::TreeModelColumn<std::string> m_colArtist;
        Gtk::TreeModelColumn<std::string> m_colAlbum;
        Gtk::TreeModelColumn<std::string> m_colDuration;
        Gtk::TreeModelColumn<std::string> m_colComment;
        Gtk::TreeModelColumn<std::string> m_colPath;
    };
}

#endif // DATAMUSICFILESCOLUMNS_H
