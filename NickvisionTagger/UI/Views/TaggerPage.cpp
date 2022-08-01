#include "TaggerPage.h"
#include <filesystem>
#include <QFileDialog>
#include <QInputDialog>
#include <QMessageBox>
#include "../Messenger.h"
#include "../Controls/ProgressDialog.h"
#include "../../Helpers/MediaHelpers.h"
#include "../../Helpers/ThemeHelpers.h"
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
		m_ui.btnRemoveTags->setVisible(false);
		m_ui.btnInsertAlbumArt->setVisible(false);
		m_ui.btnFilenameToTag->setVisible(false);
		m_ui.btnTagToFilename->setVisible(false);
		//Tag Properties
		m_ui.groupTagProperties->setVisible(false);
		//==Load Config==//
		updateConfig();
		Configuration& configuration{ Configuration::getInstance() };
		if (configuration.getRememberLastOpenedFolder() && std::filesystem::exists(configuration.getLastOpenedFolder()))
		{
			m_musicFolder.setPath(configuration.getLastOpenedFolder());
			m_ui.btnRefreshMusicFolder->setVisible(true);
			m_ui.txtStatus->setText(QString::fromStdString(m_musicFolder.getPath().string()));
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
			m_ui.txtStatus->setText(QString::fromStdString(m_musicFolder.getPath().string()));
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
		ProgressDialog savingDialog{ this, "Saving tags...", [&]()
		{
			for (const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
			{
				if (m_ui.txtFilename->text().toStdString() != musicFile->getFilename() && m_ui.txtFilename->text().toStdString() != "<keep>")
				{
					musicFile->setFilename(m_ui.txtFilename->text().toStdString());
				}
				if (m_ui.txtTitle->text().toStdString() != "<keep>")
				{
					musicFile->setTitle(m_ui.txtTitle->text().toStdString());
				}
				if (m_ui.txtArtist->text().toStdString() != "<keep>")
				{
					musicFile->setArtist(m_ui.txtArtist->text().toStdString());
				}
				if (m_ui.txtAlbum->text().toStdString() != "<keep>")
				{
					musicFile->setAlbum(m_ui.txtAlbum->text().toStdString());
				}
				if (m_ui.txtYear->value() != -1)
				{
					musicFile->setYear(m_ui.txtYear->value());
				}
				if (m_ui.txtTrack->value() != -1)
				{
					musicFile->setTrack(m_ui.txtTrack->value());
				}
				if (m_ui.txtAlbumArtist->text().toStdString() != "<keep>")
				{
					musicFile->setAlbumArtist(m_ui.txtAlbumArtist->text().toStdString());
				}
				if (m_ui.txtGenre->text().toStdString() != "<keep>")
				{
					musicFile->setGenre(m_ui.txtGenre->text().toStdString());
				}
				if (m_ui.txtComment->text().toStdString() != "<keep>")
				{
					musicFile->setComment(m_ui.txtComment->text().toStdString());
				}
				musicFile->saveTag();
			}
		}};
		savingDialog.exec();
		on_btnRefreshMusicFolder_clicked();
	}

	void TaggerPage::on_btnRemoveTags_clicked()
	{
		QMessageBox msgDeleteTags{ QMessageBox::Icon::Warning, "Remove Tags", "Are you sure you want to remove the selected tags?", QMessageBox::StandardButton::Yes | QMessageBox::StandardButton::No, this };
		ThemeHelpers::applyWin32Theme(&msgDeleteTags);
		int result{ msgDeleteTags.exec() };
		if (result == QMessageBox::StandardButton::Yes)
		{
			ProgressDialog removingDialog{ this, "Removing tags...", [&]()
			{
				for (const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
				{
					musicFile->removeTag();
				}
			}};
			removingDialog.exec();
			on_btnRefreshMusicFolder_clicked();
		}
	}

	void TaggerPage::on_btnInsertAlbumArt_clicked()
	{

	}

	void TaggerPage::on_btnFilenameToTag_clicked()
	{
		//==Format String==//
		QInputDialog formatStringDialog{ this };
		formatStringDialog.setFixedSize(320, 120);
		formatStringDialog.setWindowTitle("Filename to Tag");
		formatStringDialog.setLabelText("Select a format string: ");
		formatStringDialog.setComboBoxItems({ "%artist%- %title%", "%title%- %artist%", "%track%- %title%", "%title%" });
		ThemeHelpers::applyWin32Theme(&formatStringDialog);
		int result{ formatStringDialog.exec() };
		//==Convert==//
		if (result == QDialog::Accepted)
		{
			std::string selectedFormatString{ formatStringDialog.textValue().toStdString() };
			ProgressDialog convertingDialog{ this, "Converting filenames to tags...", [&]()
			{
				for (const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
				{
					musicFile->filenameToTag(selectedFormatString);
				}
			}};
			convertingDialog.exec();
			on_btnRefreshMusicFolder_clicked();
		}
	}

	void TaggerPage::on_btnTagToFilename_clicked()
	{
		//==Format String==//
		QInputDialog formatStringDialog{ this };
		formatStringDialog.setFixedSize(320, 120);
		formatStringDialog.setWindowTitle("Tag to Filename");
		formatStringDialog.setLabelText("Select a format string: ");
		formatStringDialog.setComboBoxItems({ "%artist%- %title%", "%title%- %artist%", "%track%- %title%", "%title%" });
		ThemeHelpers::applyWin32Theme(&formatStringDialog);
		int result{ formatStringDialog.exec() };
		//==Convert==//
		if (result == QDialog::Accepted)
		{
			std::string selectedFormatString{ formatStringDialog.textValue().toStdString() };
			ProgressDialog convertingDialog{ this, "Converting tags to filenames...", [&]()
			{
				for (const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
				{
					musicFile->tagToFilename(selectedFormatString);
				}
			}};
			convertingDialog.exec();
			on_btnRefreshMusicFolder_clicked();
		}
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
		m_ui.btnRemoveTags->setVisible(true);
		m_ui.btnInsertAlbumArt->setVisible(true);
		m_ui.btnFilenameToTag->setVisible(true);
		m_ui.btnTagToFilename->setVisible(true);
		m_ui.groupTagProperties->setVisible(true);
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
			m_ui.btnRemoveTags->setVisible(false);
			m_ui.btnInsertAlbumArt->setVisible(false);
			m_ui.btnFilenameToTag->setVisible(false);
			m_ui.btnTagToFilename->setVisible(false);
			m_ui.groupTagProperties->setVisible(false);
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
