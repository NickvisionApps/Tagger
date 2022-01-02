#include "mainwindow.h"
#include <stdexcept>
#include <filesystem>
#include "../models/configuration.h"
#include "../models/update/updater.h"
#include "settingsdialog.h"
#include "../helpers/mediahelpers.h"

unsigned int stoui(const std::string& str, size_t* idx = 0, int base = 10)
{
    unsigned long ui = std::stoul(str, idx, base);
    if (ui > UINT_MAX)
    {
        throw std::out_of_range(str);
    }
    return ui;
}

namespace NickvisionTagger::Views
{
    using namespace NickvisionTagger::Models;
    using namespace NickvisionTagger::Models::Update;
    using namespace NickvisionTagger::Helpers;

    MainWindow::MainWindow()
    {
        //==Settings==//
        set_default_size(800, 600);
        set_title("Nickvision Tagger");
        set_titlebar(m_headerBar);
        //==HeaderBar==//
        m_headerBar.getActionOpenMusicFolder()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::openMusicFolder));
        m_headerBar.getActionReloadMusicFolder()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::reloadMusicFolder));
        m_headerBar.getActionCloseMusicFolder()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::closeMusicFolder));
        m_headerBar.getBtnSaveTags().signal_clicked().connect(sigc::mem_fun(*this, &MainWindow::saveTags));
        m_headerBar.getBtnRTRemove().signal_clicked().connect(sigc::mem_fun(*this, &MainWindow::removeTags));
        m_headerBar.getBtnFTTConvert().signal_clicked().connect(sigc::mem_fun(*this, &MainWindow::filenameToTag));
        m_headerBar.getBtnTTFConvert().signal_clicked().connect(sigc::mem_fun(*this, &MainWindow::tagToFilename));
        m_headerBar.getBtnSettings().signal_clicked().connect(sigc::mem_fun(*this, &MainWindow::settings));
        m_headerBar.getActionCheckForUpdates()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::checkForUpdates));
        m_headerBar.getActionGitHubRepo()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::gitHubRepo));
        m_headerBar.getActionReportABug()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::reportABug));
        m_headerBar.getActionChangelog()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::changelog));
        m_headerBar.getActionAbout()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::about));
        m_headerBar.getActionReloadMusicFolder()->set_enabled(false);
        m_headerBar.getActionCloseMusicFolder()->set_enabled(false);
        m_headerBar.getBtnSaveTags().set_sensitive(false);
        m_headerBar.getBtnRemoveTags().set_sensitive(false);
        m_headerBar.getBtnFilenameToTag().set_sensitive(false);
        m_headerBar.getBtnTagToFilename().set_sensitive(false);
        //==Music Properties==//
        m_boxMusicProperties.set_orientation(Gtk::Orientation::VERTICAL);
        m_boxMusicProperties.set_spacing(6);
        m_boxMusicProperties.set_margin_end(20);
        //Filename
        m_lblFilename.set_label("Filename");
        m_lblFilename.set_halign(Gtk::Align::START);
        m_txtFilename.set_placeholder_text("Enter filename here");
        m_boxMusicProperties.append(m_lblFilename);
        m_boxMusicProperties.append(m_txtFilename);
        //Title
        m_lblTitle.set_label("Title");
        m_lblTitle.set_halign(Gtk::Align::START);
        m_txtTitle.set_placeholder_text("Enter title here");
        m_boxMusicProperties.append(m_lblTitle);
        m_boxMusicProperties.append(m_txtTitle);
        //Artist
        m_lblArtist.set_label("Artist");
        m_lblArtist.set_halign(Gtk::Align::START);
        m_txtArtist.set_placeholder_text("Enter artist here");
        m_boxMusicProperties.append(m_lblArtist);
        m_boxMusicProperties.append(m_txtArtist);
        //Album
        m_lblAlbum.set_label("Album");
        m_lblAlbum.set_halign(Gtk::Align::START);
        m_txtAlbum.set_placeholder_text("Enter album here");
        m_boxMusicProperties.append(m_lblAlbum);
        m_boxMusicProperties.append(m_txtAlbum);
        //Year
        m_lblYear.set_label("Year");
        m_lblYear.set_halign(Gtk::Align::START);
        m_txtYear.set_placeholder_text("Enter year here");
        m_boxMusicProperties.append(m_lblYear);
        m_boxMusicProperties.append(m_txtYear);
        //Track
        m_lblTrack.set_label("Track");
        m_lblTrack.set_halign(Gtk::Align::START);
        m_txtTrack.set_placeholder_text("Enter track here");
        m_boxMusicProperties.append(m_lblTrack);
        m_boxMusicProperties.append(m_txtTrack);
        //Genre
        m_lblGenre.set_label("Genre");
        m_lblGenre.set_halign(Gtk::Align::START);
        m_txtGenre.set_placeholder_text("Enter genre here");
        m_boxMusicProperties.append(m_lblGenre);
        m_boxMusicProperties.append(m_txtGenre);
        //Comment
        m_lblComment.set_label("Comment");
        m_lblComment.set_halign(Gtk::Align::START);
        m_txtComment.set_placeholder_text("Enter comment here");
        m_boxMusicProperties.append(m_lblComment);
        m_boxMusicProperties.append(m_txtComment);
        //Duration
        m_lblDuration.set_label("Duration");
        m_lblDuration.set_halign(Gtk::Align::START);
        m_txtDuration.set_placeholder_text("00:00:00");
        m_txtDuration.set_editable(false);
        m_boxMusicProperties.append(m_lblDuration);
        m_boxMusicProperties.append(m_txtDuration);
        //File Size
        m_lblFileSize.set_label("File Size");
        m_lblFileSize.set_halign(Gtk::Align::START);
        m_txtFileSize.set_placeholder_text("0 MB");
        m_txtFileSize.set_editable(false);
        m_boxMusicProperties.append(m_lblFileSize);
        m_boxMusicProperties.append(m_txtFileSize);
        //ScrolledWindow
        m_scrollMusicProperties.set_size_request(380, -1);
        m_scrollMusicProperties.set_child(m_boxMusicProperties);
        //==Data Music Files==//
        m_dataMusicFilesModel = Gtk::ListStore::create(m_dataMusicFilesColumns);
        m_dataMusicFiles.append_column("ID", m_dataMusicFilesColumns.getColID());
        m_dataMusicFiles.append_column("Filename", m_dataMusicFilesColumns.getColFilename());
        m_dataMusicFiles.append_column("Title", m_dataMusicFilesColumns.getColTitle());
        m_dataMusicFiles.append_column("Artist", m_dataMusicFilesColumns.getColArtist());
        m_dataMusicFiles.append_column("Album", m_dataMusicFilesColumns.getColAlbum());
        m_dataMusicFiles.append_column("Duration", m_dataMusicFilesColumns.getColDuration());
        m_dataMusicFiles.append_column("Comment", m_dataMusicFilesColumns.getColComment());
        m_dataMusicFiles.append_column("Path", m_dataMusicFilesColumns.getColPath());
        m_dataMusicFiles.set_model(m_dataMusicFilesModel);
        m_dataMusicFiles.get_selection()->set_mode(Gtk::SelectionMode::MULTIPLE);
        m_dataMusicFiles.get_selection()->signal_changed().connect(sigc::mem_fun(*this, &MainWindow::dataMusicFilesSelectionChanged));
        //ScrolledWindow
        m_scrollDataMusicFiles.set_child(m_dataMusicFiles);
        m_scrollDataMusicFiles.set_margin_start(6);
        m_scrollDataMusicFiles.set_expand(true);
        //==Layout==//
        m_boxWindow.set_orientation(Gtk::Orientation::HORIZONTAL);
        m_boxWindow.set_margin(6);
        m_boxWindow.append(m_scrollMusicProperties);
        m_boxWindow.append(m_scrollDataMusicFiles);
        m_mainBox.set_orientation(Gtk::Orientation::VERTICAL);
        m_mainBox.append(m_infoBar);
        m_mainBox.append(m_boxWindow);
        set_child(m_mainBox);
        maximize();
        //==Load Config==//
        Configuration configuration;
        m_musicFolder.setIncludeSubfolders(configuration.includeSubfolders());
        if(configuration.rememberLastOpenedFolder() && std::filesystem::exists(configuration.getLastOpenedFolder()))
        {
            m_musicFolder.setPath(configuration.getLastOpenedFolder());
            set_title("Nickvision Tagger (" + m_musicFolder.getPath() + ")");
            m_headerBar.getActionReloadMusicFolder()->set_enabled(true);
            m_headerBar.getActionCloseMusicFolder()->set_enabled(true);
            reloadMusicFolder({});
        }
    }

    MainWindow::~MainWindow()
    {
        //==Save Config==//
        Configuration configuration;
        if(configuration.rememberLastOpenedFolder())
        {
            configuration.setLastOpenedFolder(m_musicFolder.getPath());
        }
        configuration.save();
    }

    void MainWindow::openMusicFolder(const Glib::VariantBase& args)
    {
        Gtk::FileChooserDialog* folderDialog = new Gtk::FileChooserDialog(*this, "Select Folder", Gtk::FileChooserDialog::Action::SELECT_FOLDER, true);
        folderDialog->set_modal(true);
        folderDialog->add_button("_Select", Gtk::ResponseType::OK);
        folderDialog->add_button("_Cancel", Gtk::ResponseType::CANCEL);
        folderDialog->signal_response().connect(sigc::bind([&](int response, Gtk::FileChooserDialog* dialog)
        {
            if(response == Gtk::ResponseType::OK)
            {
                m_musicFolder.setPath(dialog->get_file()->get_path());
                set_title("Nickvision Tagger (" + m_musicFolder.getPath() + ")");
                m_headerBar.getActionReloadMusicFolder()->set_enabled(true);
                m_headerBar.getActionCloseMusicFolder()->set_enabled(true);
                reloadMusicFolder({});
            }
            delete dialog;
        }, folderDialog));
        folderDialog->show();
    }

    void MainWindow::reloadMusicFolder(const Glib::VariantBase& args)
    {
        m_dataMusicFilesModel->clear();
        m_infoBar.showMessage("Please Wait", "Loading music files...", false);
        m_musicFolder.reloadFiles();
        for(size_t i = 0; i < m_musicFolder.getFiles().size(); i++)
        {
            m_infoBar.showMessage("Please Wait", "Loading music files (" + std::to_string(i + 1) + " / " + std::to_string(m_musicFolder.getFiles().size()) + ")...", false);
            Gtk::TreeRow row = *(m_dataMusicFilesModel->append());
            row[m_dataMusicFilesColumns.getColID()] = i + 1;
            row[m_dataMusicFilesColumns.getColFilename()] = m_musicFolder.getFiles()[i]->getFilename();
            row[m_dataMusicFilesColumns.getColTitle()] = m_musicFolder.getFiles()[i]->getTitle();
            row[m_dataMusicFilesColumns.getColArtist()] = m_musicFolder.getFiles()[i]->getArtist();
            row[m_dataMusicFilesColumns.getColAlbum()] = m_musicFolder.getFiles()[i]->getAlbum();
            row[m_dataMusicFilesColumns.getColDuration()] = m_musicFolder.getFiles()[i]->getDurationAsString();
            row[m_dataMusicFilesColumns.getColComment()] = m_musicFolder.getFiles()[i]->getComment();
            row[m_dataMusicFilesColumns.getColPath()] = m_musicFolder.getFiles()[i]->getPath();
        }
        m_dataMusicFiles.columns_autosize();
        m_infoBar.hide();
    }

    void MainWindow::closeMusicFolder(const Glib::VariantBase& args)
    {
        m_musicFolder.setPath("");
        set_title("Nickvision Tagger");
        m_headerBar.getActionReloadMusicFolder()->set_enabled(false);
        m_headerBar.getActionCloseMusicFolder()->set_enabled(false);
        reloadMusicFolder({});
    }

    void MainWindow::saveTags()
    {
        for(size_t i = 0; i < m_selectedMusicFiles.size(); i++)
        {
            m_infoBar.showMessage("Please Wait", "Saving tags (" + std::to_string(i + 1) + " / " + std::to_string(m_selectedMusicFiles.size()) + ")...", false);
            if(std::string(m_txtFilename.get_text()) != m_selectedMusicFiles[i]->getFilename() && m_txtFilename.get_text() != "<keep>")
            {
                m_selectedMusicFiles[i]->setFilename(m_txtFilename.get_text());
            }
            if(m_txtTitle.get_text() != "<keep>")
            {
                m_selectedMusicFiles[i]->setTitle(m_txtTitle.get_text());
            }
            if(m_txtArtist.get_text() != "<keep>")
            {
                m_selectedMusicFiles[i]->setArtist(m_txtArtist.get_text());
            }
            if(m_txtAlbum.get_text() != "<keep>")
            {
                m_selectedMusicFiles[i]->setAlbum(m_txtAlbum.get_text());
            }
            if(m_txtYear.get_text() != "<keep>")
            {
                try
                {
                    m_selectedMusicFiles[i]->setYear(stoui(m_txtYear.get_text()));
                }
                catch(...) { }
            }
            if(m_txtTrack.get_text() != "<keep>")
            {
                try
                {
                    m_selectedMusicFiles[i]->setTrack(stoui(m_txtTrack.get_text()));
                }
                catch(...) { }
            }
            if(m_txtGenre.get_text() != "<keep>")
            {
                m_selectedMusicFiles[i]->setGenre(m_txtGenre.get_text());
            }
            if(m_txtComment.get_text() != "<keep>")
            {
                m_selectedMusicFiles[i]->setComment(m_txtComment.get_text());
            }
            m_selectedMusicFiles[i]->saveTag();
        }
        reloadMusicFolder({});
    }

    void MainWindow::removeTags()
    {
        m_headerBar.getPopRemoveTags().popdown();
        for(size_t i = 0; i < m_selectedMusicFiles.size(); i++)
        {
            m_infoBar.showMessage("Please Wait", "Removing tags (" + std::to_string(i + 1) + " / " + std::to_string(m_selectedMusicFiles.size()) + ")...", false);
            m_selectedMusicFiles[i]->removeTag();
        }
        reloadMusicFolder({});
    }

    void MainWindow::filenameToTag()
    {
        m_headerBar.getPopFilenameToTag().popdown();
        std::string formatString = m_headerBar.getCmbFTTFormatString().get_active_text();
        int success = 0;
        std::string total = std::to_string(m_selectedMusicFiles.size());
        for(size_t i = 0; i < m_selectedMusicFiles.size(); i++)
        {
            m_infoBar.showMessage("Please Wait", "Converting filenames to tags (" + std::to_string(i + 1) + " / " + total + ")...", false);
            try
            {
                m_selectedMusicFiles[i]->filenameToTag(formatString);
                success++;
            }
            catch (...) { }
        }
        reloadMusicFolder({});
        m_infoBar.showMessage("Conversion Complete", "Converted " + std::to_string(success) + " out of " + total + " filenames to tags.");
    }

    void MainWindow::tagToFilename()
    {
         m_headerBar.getPopTagToFilename().popdown();
         std::string formatString = m_headerBar.getCmbTTFFormatString().get_active_text();
         int success = 0;
         std::string total = std::to_string(m_selectedMusicFiles.size());
         for(size_t i = 0; i < m_selectedMusicFiles.size(); i++)
         {
             m_infoBar.showMessage("Please Wait", "Converting tags to filenames (" + std::to_string(i + 1) + " / " + total + ")...", false);
             try
             {
                 m_selectedMusicFiles[i]->tagToFilename(formatString);
                 success++;
             }
             catch (...) { }
         }
         reloadMusicFolder({});
         m_infoBar.showMessage("Conversion Complete", "Converted " + std::to_string(success) + " out of " + total + " tags to filenames.");
    }

    void MainWindow::settings()
    {
        SettingsDialog* settingsDialog = new SettingsDialog(*this);
        settingsDialog->signal_hide().connect(sigc::bind([&](SettingsDialog* dialog)
        {
            delete dialog;
            Configuration configuration;
            m_musicFolder.setIncludeSubfolders(configuration.includeSubfolders());
            reloadMusicFolder({});
        }, settingsDialog));
        settingsDialog->show();
    }

    void MainWindow::checkForUpdates(const Glib::VariantBase& args)
    {
        Updater updater("https://raw.githubusercontent.com/nlogozzo/NickvisionTagger/main/UpdateConfig.json", { "2022.1.0" });
        m_infoBar.showMessage("Please Wait", "Checking for updates...", false);
        updater.checkForUpdates();
        m_infoBar.hide();
        if(updater.updateAvailable())
        {
            Gtk::MessageDialog* updateDialog = new Gtk::MessageDialog(*this, "Update Available", false, Gtk::MessageType::INFO, Gtk::ButtonsType::OK, true);
            updateDialog->set_secondary_text("\n===V" + updater.getLatestVersion()->toString() + " Changelog===\n" + updater.getChangelog() + "\n\nPlease visit the GitHub repo or update through your package manager to get the latest version.");
            updateDialog->signal_response().connect(sigc::bind([](int response, Gtk::MessageDialog* dialog)
            {
               delete dialog;
            }, updateDialog));
            updateDialog->show();
        }
        else
        {
            m_infoBar.showMessage("No Update Available", "There is no update at this time. Please check again later.");
        }
    }

    void MainWindow::gitHubRepo(const Glib::VariantBase& args)
    {
        system("xdg-open https://github.com/nlogozzo/NickvisionTagger");
    }

    void MainWindow::reportABug(const Glib::VariantBase& args)
    {
        system("xdg-open https://github.com/nlogozzo/NickvisionTagger/issues/new");
    }

    void MainWindow::changelog(const Glib::VariantBase& args)
    {
        Gtk::MessageDialog* changelogDialog = new Gtk::MessageDialog(*this, "What's New?", false, Gtk::MessageType::INFO, Gtk::ButtonsType::OK, true);
        changelogDialog->set_secondary_text("\n- Rewrote Tagger in C++ using gtkmm\n- Removed support for Windows OS. Only Linux is now supported");
        changelogDialog->signal_response().connect(sigc::bind([](int response, Gtk::MessageDialog* dialog)
        {
           delete dialog;
        }, changelogDialog));
        changelogDialog->show();
    }

    void MainWindow::about(const Glib::VariantBase& args)
    {
        Gtk::AboutDialog* aboutDialog = new Gtk::AboutDialog();
        aboutDialog->set_transient_for(*this);
        aboutDialog->set_modal(true);
        aboutDialog->set_hide_on_close(true);
        aboutDialog->set_program_name("Nickvision Tagger");
        aboutDialog->set_version("2022.1.0");
        aboutDialog->set_comments("An easy to use music tag (metadata) editor.");
        aboutDialog->set_copyright("(C) Nickvision 2021-2022");
        aboutDialog->set_license_type(Gtk::License::GPL_3_0);
        aboutDialog->set_website("https://github.com/nlogozzo");
        aboutDialog->set_website_label("GitHub");
        aboutDialog->set_authors({ "Nicholas Logozzo" });
        aboutDialog->signal_hide().connect(sigc::bind([](Gtk::AboutDialog* dialog)
        {
           delete dialog;
        }, aboutDialog));
        aboutDialog->show();
    }

    void MainWindow::dataMusicFilesSelectionChanged()
    {
        //==Update Selected Music Files==//
        m_selectedMusicFiles.clear();
        for(const Gtk::TreeModel::Path& row : m_dataMusicFiles.get_selection()->get_selected_rows())
        {
            m_selectedMusicFiles.push_back(m_musicFolder.getFiles()[row[0]]);
        }
        //==Update UI==//
        m_txtFilename.set_editable(true);
        m_headerBar.getBtnSaveTags().set_sensitive(true);
        m_headerBar.getBtnRemoveTags().set_sensitive(true);
        m_headerBar.getBtnFilenameToTag().set_sensitive(true);
        m_headerBar.getBtnTagToFilename().set_sensitive(true);
        //==No Files Selected==//
        if(m_selectedMusicFiles.size() == 0)
        {
            m_txtFilename.set_text("");
            m_txtTitle.set_text("");
            m_txtArtist.set_text("");
            m_txtAlbum.set_text("");
            m_txtYear.set_text("");
            m_txtTrack.set_text("");
            m_txtGenre.set_text("");
            m_txtComment.set_text("");
            m_txtDuration.set_text("");
            m_txtFileSize.set_text("");
            m_headerBar.getBtnSaveTags().set_sensitive(false);
            m_headerBar.getBtnRemoveTags().set_sensitive(false);
            m_headerBar.getBtnFilenameToTag().set_sensitive(false);
            m_headerBar.getBtnTagToFilename().set_sensitive(false);
        }
        //==One File Selected==//
        else if(m_selectedMusicFiles.size() == 1)
        {
            m_txtFilename.set_text(m_selectedMusicFiles[0]->getFilename());
            m_txtTitle.set_text(m_selectedMusicFiles[0]->getTitle());
            m_txtArtist.set_text(m_selectedMusicFiles[0]->getArtist());
            m_txtAlbum.set_text(m_selectedMusicFiles[0]->getAlbum());
            m_txtYear.set_text(std::to_string(m_selectedMusicFiles[0]->getYear()));
            m_txtTrack.set_text(std::to_string(m_selectedMusicFiles[0]->getTrack()));
            m_txtGenre.set_text(m_selectedMusicFiles[0]->getGenre());
            m_txtComment.set_text(m_selectedMusicFiles[0]->getComment());
            m_txtDuration.set_text(m_selectedMusicFiles[0]->getDurationAsString());
            m_txtFileSize.set_text(m_selectedMusicFiles[0]->getFileSizeAsString());
        }
        //==Multiple Files Selected==//
        else
        {
            bool haveSameTitle = true;
            bool haveSameArtist = true;
            bool haveSameAlbum = true;
            bool haveSameYear = true;
            bool haveSameTrack = true;
            bool haveSameGenre = true;
            bool haveSameComment = true;
            int totalDuration = 0;
            std::uintmax_t totalFileSize = 0;
            for(const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
            {
                if (m_selectedMusicFiles[0]->getTitle() != musicFile->getTitle())
                {
                    haveSameTitle = false;
                }
                if (m_selectedMusicFiles[0]->getArtist() != musicFile->getArtist())
                {
                    haveSameArtist = false;
                }
                if (m_selectedMusicFiles[0]->getAlbum() != musicFile->getAlbum())
                {
                    haveSameAlbum = false;
                }
                if (m_selectedMusicFiles[0]->getYear() != musicFile->getYear())
                {
                    haveSameYear = false;
                }
                if (m_selectedMusicFiles[0]->getTrack() != musicFile->getTrack())
                {
                    haveSameTrack = false;
                }
                if (m_selectedMusicFiles[0]->getGenre() != musicFile->getGenre())
                {
                    haveSameGenre = false;
                }
                if (m_selectedMusicFiles[0]->getComment() != musicFile->getComment())
                {
                    haveSameComment = false;
                }
                totalDuration += musicFile->getDuration();
                totalFileSize += musicFile->getFileSize();
            }
            m_txtFilename.set_editable(false);
            m_txtFilename.set_text("<keep>");
            m_txtTitle.set_text(haveSameTitle ? m_selectedMusicFiles[0]->getTitle() : "<keep>");
            m_txtArtist.set_text(haveSameArtist ? m_selectedMusicFiles[0]->getArtist() : "<keep>");
            m_txtAlbum.set_text(haveSameAlbum ? m_selectedMusicFiles[0]->getAlbum() : "<keep>");
            m_txtYear.set_text(haveSameYear ? std::to_string(m_selectedMusicFiles[0]->getYear()) : "<keep>");
            m_txtTrack.set_text(haveSameTrack ? std::to_string(m_selectedMusicFiles[0]->getTrack()) : "<keep>");
            m_txtGenre.set_text(haveSameGenre ? m_selectedMusicFiles[0]->getGenre() : "<keep>");
            m_txtComment.set_text(haveSameComment ? m_selectedMusicFiles[0]->getComment() : "<keep>");
            m_txtDuration.set_text(MediaHelpers::durationToString(totalDuration));
            m_txtFileSize.set_text(MediaHelpers::fileSizeToString(totalFileSize));
        }
    }
}
