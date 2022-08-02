#pragma once

#include <vector>
#include <QWidget>
#include "ui_TaggerPage.h"
#include "../../Models/MusicFile.h"
#include "../../Models/MusicFolder.h"

namespace NickvisionTagger::UI::Views
{
	/// <summary>
	/// The tagger page for the application
	/// </summary>
	class TaggerPage : public QWidget
	{
		Q_OBJECT

	public:
		/// <summary>
		/// Constructs a TaggerPage
		/// </summary>
		/// <param name="parent">The parent of the widget, if any</param>
		TaggerPage(QWidget* parent = nullptr);
		/// <summary>
		/// Refreshes the theme of the page
		/// </summary>
		void refreshTheme();
		/// <summary>
		/// Updates the page's objects with the latest configuration
		/// </summary>
		void updateConfig();

	protected:
		void showEvent(QShowEvent* event) override;

	private slots:
		/// <summary>
		/// Prompts the user to open a music folder
		/// </summary>
		void on_btnOpenMusicFolder_clicked();
		/// <summary>
		/// Rescans the music folder for music files
		/// </summary>
		void on_btnRefreshMusicFolder_clicked();
		/// <summary>
		/// Closes the music folder
		/// </summary>
		void on_btnCloseMusicFolder_clicked();
		/// <summary>
		/// Saves the tags of the selected music files
		/// </summary>
		void on_btnSaveTags_clicked();
		/// <summary>
		/// Removes the tags of the selected music files
		/// </summary>
		void on_btnRemoveTags_clicked();
		/// <summary>
		/// Prompts the user to select an image to use as a music file's album art
		/// </summary>
		void on_btnInsertAlbumArt_clicked();
		/// <summary>
		/// Converts selected music files' filename into it's tag
		/// </summary>
		void on_btnFilenameToTag_clicked();
		/// <summary>
		/// Converts selected music files' tag into it's filename
		/// </summary>
		void on_btnTagToFilename_clicked();
		/// <summary>
		/// Updates the selected music files
		/// </summary>
		void on_tblMusicFiles_itemSelectionChanged();

	private:
		//==Vars==//
		bool m_opened;
		NickvisionTagger::Models::MusicFolder m_musicFolder;
		std::vector<std::shared_ptr<NickvisionTagger::Models::MusicFile>> m_selectedMusicFiles;
		//==UI==//
		Ui::TaggerPage m_ui;
	};
}
