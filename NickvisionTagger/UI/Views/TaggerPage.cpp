#include "TaggerPage.h"
#include <filesystem>
#include <QFileDialog>
#include "../Messenger.h"
#include "../Controls/ProgressDialog.h"
#include "../../Helpers/MediaHelpers.h"
#include "../../Models/Configuration.h"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;
using namespace NickvisionTagger::UI;
using namespace NickvisionTagger::UI::Controls;

namespace NickvisionTagger::UI::Views
{
	TaggerPage::TaggerPage(QWidget* parent) : QWidget(parent)
	{
		//==UI==//
		m_ui.setupUi(this);
		//Buttons
		m_ui.btnRefreshMusicFolder->setVisible(false);
		m_ui.btnSaveTags->setVisible(false);
		m_ui.btnDeleteTags->setVisible(false);
		m_ui.btnInsertAlbumArt->setVisible(false);
		m_ui.btnFilenameToTag->setVisible(false);
		m_ui.btnTagToFilename->setVisible(false);
		//==Load Config==//
		updateConfig();
		Configuration& configuration{ Configuration::getInstance() };
		if (configuration.getRememberLastOpenedFolder() && std::filesystem::exists(configuration.getLastOpenedFolder()))
		{
			m_musicFolder.setPath(configuration.getLastOpenedFolder());
			m_ui.btnRefreshMusicFolder->setVisible(true);
			on_btnRefreshMusicFolder_clicked();
		}
	}

	void TaggerPage::updateConfig()
	{
		Configuration& configuration{ Configuration::getInstance() };
		if (m_musicFolder.getIncludeSubfolders() != configuration.getIncludeSubfolders())
		{
			m_musicFolder.setIncludeSubfolders(configuration.getIncludeSubfolders());
			on_btnRefreshMusicFolder_clicked();
		}
	}

	void TaggerPage::on_btnOpenMusicFolder_clicked()
	{
		std::string folderPath{ QFileDialog::getExistingDirectory(this, "Open Music Folder").toStdString() };
		if (!folderPath.empty())
		{
			m_musicFolder.setPath(folderPath);
			//==Update Config==//
			Configuration& configuration{ Configuration::getInstance() };
			if (configuration.getRememberLastOpenedFolder())
			{
				configuration.setLastOpenedFolder(m_musicFolder.getPath().string());
				configuration.save();
			}
			//==Update UI==//
			m_ui.btnRefreshMusicFolder->setVisible(true);
			on_btnRefreshMusicFolder_clicked();
		}
	}

	void TaggerPage::on_btnRefreshMusicFolder_clicked()
	{
		m_ui.tblMusicFiles->clear();
		//==Load Files==//
		ProgressDialog loadingDialog{ this, "Loading music files...", [&]() { m_musicFolder.reloadFiles(); } };
		loadingDialog.exec();
		//==Setup Table==//
		m_ui.tblMusicFiles->setColumnCount(7);
		m_ui.tblMusicFiles->setRowCount(m_musicFolder.getFiles().size());
		m_ui.tblMusicFiles->setHorizontalHeaderLabels({ "Filename", "Title", "Artist", "Album", "Comment", "Duration", "Path" });
		//==Load Files In Table==//
		int id = 0;
		for (const std::shared_ptr<MusicFile>& musicFile : m_musicFolder.getFiles())
		{
			m_ui.tblMusicFiles->setItem(id, 0, new QTableWidgetItem(QString::fromStdString(musicFile->getFilename())));
			m_ui.tblMusicFiles->setItem(id, 1, new QTableWidgetItem(QString::fromStdString(musicFile->getTitle())));
			m_ui.tblMusicFiles->setItem(id, 2, new QTableWidgetItem(QString::fromStdString(musicFile->getArtist())));
			m_ui.tblMusicFiles->setItem(id, 3, new QTableWidgetItem(QString::fromStdString(musicFile->getAlbum())));
			m_ui.tblMusicFiles->setItem(id, 4, new QTableWidgetItem(QString::fromStdString(musicFile->getComment())));
			m_ui.tblMusicFiles->setItem(id, 5, new QTableWidgetItem(QString::fromStdString(musicFile->getDurationAsString())));
			m_ui.tblMusicFiles->setItem(id, 6, new QTableWidgetItem(QString::fromStdString(musicFile->getPath().string())));
			++id;
		}
		m_ui.tblMusicFiles->resizeColumnsToContents();
	}

	void TaggerPage::on_btnSaveTags_clicked()
	{

	}

	void TaggerPage::on_btnDeleteTags_clicked()
	{

	}

	void TaggerPage::on_btnInsertAlbumArt_clicked()
	{

	}

	void TaggerPage::on_btnFilenameToTag_clicked()
	{

	}

	void TaggerPage::on_btnTagToFilename_clicked()
	{

	}

	void TaggerPage::on_tblMusicFiles_itemSelectionChanged()
	{
		//==Update Selected Music Files==//
		m_selectedMusicFiles.clear();
		for (QTableWidgetItem* item : m_ui.tblMusicFiles->selectedItems())
		{
			if (item->column() != 0)
			{
				continue;
			}
			m_selectedMusicFiles.push_back(m_musicFolder.getFiles()[item->row()]);
		}
		//==Update UI==//
		m_ui.btnSaveTags->setVisible(true);
		m_ui.btnDeleteTags->setVisible(true);
		m_ui.btnInsertAlbumArt->setVisible(true);
		m_ui.btnFilenameToTag->setVisible(true);
		m_ui.btnTagToFilename->setVisible(true);
		m_ui.txtFilename->setReadOnly(false);
		//==No Files Selected==//
		if (m_selectedMusicFiles.size() == 0)
		{
			m_ui.txtFilename->setText("");
			m_ui.txtTitle->setText("");
			m_ui.txtArtist->setText("");
			m_ui.txtAlbum->setText("");
			m_ui.txtYear->setValue(0);
			m_ui.txtTrack->setValue(0);
			m_ui.txtAlbumArtist->setText("");
			m_ui.txtGenre->setText("");
			m_ui.txtComment->setText("");
			m_ui.txtDuration->setText("");
			m_ui.txtFileSize->setText("");
			m_ui.imgAlbumArt->setPixmap({});
			m_ui.btnSaveTags->setVisible(false);
			m_ui.btnDeleteTags->setVisible(false);
			m_ui.btnInsertAlbumArt->setVisible(false);
			m_ui.btnFilenameToTag->setVisible(false);
			m_ui.btnTagToFilename->setVisible(false);
		}
		//==One File Selected==//
		else if (m_selectedMusicFiles.size() == 1)
		{
			const std::shared_ptr<MusicFile>& firstMusicFile{ m_selectedMusicFiles[0] };
			m_ui.txtFilename->setText(QString::fromStdString(firstMusicFile->getFilename()));
			m_ui.txtTitle->setText(QString::fromStdString(firstMusicFile->getTitle()));
			m_ui.txtArtist->setText(QString::fromStdString(firstMusicFile->getArtist()));
			m_ui.txtAlbum->setText(QString::fromStdString(firstMusicFile->getAlbum()));
			m_ui.txtYear->setValue(firstMusicFile->getYear());
			m_ui.txtTrack->setValue(firstMusicFile->getTrack());
			m_ui.txtAlbumArtist->setText(QString::fromStdString(firstMusicFile->getAlbumArtist()));
			m_ui.txtGenre->setText(QString::fromStdString(firstMusicFile->getGenre()));
			m_ui.txtComment->setText(QString::fromStdString(firstMusicFile->getComment()));
			m_ui.txtDuration->setText(QString::fromStdString(firstMusicFile->getDurationAsString()));
			m_ui.txtFileSize->setText(QString::fromStdString(firstMusicFile->getFileSizeAsString()));
			m_ui.imgAlbumArt->setPixmap({});
		}
		//==Multiple Files Selected==//
		else
		{
			const std::shared_ptr<MusicFile>& firstMusicFile{ m_selectedMusicFiles[0] };
			bool haveSameTitle{ true };
			bool haveSameArtist{ true };
			bool haveSameAlbum{ true };
			bool haveSameYear{ true };
			bool haveSameTrack{ true };
			bool haveSameAlbumArtist{ true };
			bool haveSameGenre{ true };
			bool haveSameComment{ true };
			bool haveSameAlbumArt{ true };
			int totalDuration{ 0 };
			std::uintmax_t totalFileSize{ 0 };
			for (const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
			{
				if (firstMusicFile->getTitle() != musicFile->getTitle())
				{
					haveSameTitle = false;
				}
				if (firstMusicFile->getArtist() != musicFile->getArtist())
				{
					haveSameArtist = false;
				}
				if (firstMusicFile->getAlbum() != musicFile->getAlbum())
				{
					haveSameAlbum = false;
				}
				if (firstMusicFile->getYear() != musicFile->getYear())
				{
					haveSameYear = false;
				}
				if (firstMusicFile->getTrack() != musicFile->getTrack())
				{
					haveSameTrack = false;
				}
				if (firstMusicFile->getAlbumArtist() != musicFile->getAlbumArtist())
				{
					haveSameAlbumArtist = false;
				}
				if (firstMusicFile->getGenre() != musicFile->getGenre())
				{
					haveSameGenre = false;
				}
				if (firstMusicFile->getComment() != musicFile->getComment())
				{
					haveSameComment = false;
				}
				if (firstMusicFile->getAlbumArt() != musicFile->getAlbumArt())
				{
					haveSameAlbumArt = false;
				}
				totalDuration += musicFile->getDuration();
				totalFileSize += musicFile->getFileSize();
			}
			m_ui.txtFilename->setReadOnly(true);
			m_ui.txtFilename->setText("<keep>");
			m_ui.txtTitle->setText(haveSameTitle ? QString::fromStdString(firstMusicFile->getTitle()) : "<keep>");
			m_ui.txtArtist->setText(haveSameArtist ? QString::fromStdString(firstMusicFile->getArtist()) : "<keep>");
			m_ui.txtAlbum->setText(haveSameAlbum ? QString::fromStdString(firstMusicFile->getAlbum()) : "<keep>");
			m_ui.txtYear->setValue(haveSameYear ? firstMusicFile->getYear() : -1);
			m_ui.txtTrack->setValue(haveSameTrack ? firstMusicFile->getTrack() : -1);
			m_ui.txtAlbumArtist->setText(haveSameAlbumArtist ? QString::fromStdString(firstMusicFile->getAlbumArtist()) : "<keep>");
			m_ui.txtGenre->setText(haveSameGenre ? QString::fromStdString(firstMusicFile->getGenre()) : "<keep>");
			m_ui.txtComment->setText(haveSameComment ? QString::fromStdString(firstMusicFile->getComment()) : "<keep>");
			m_ui.txtDuration->setText(QString::fromStdString(MediaHelpers::durationToString(totalDuration)));
			m_ui.txtFileSize->setText(QString::fromStdString(MediaHelpers::fileSizeToString(totalFileSize)));
			m_ui.imgAlbumArt->setPixmap(haveSameAlbumArt ? QPixmap() : QPixmap());
		}
	}
}
