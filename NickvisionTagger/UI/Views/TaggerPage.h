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
		void updateConfig();

	private slots:
		void on_btnOpenMusicFolder_clicked();
		void on_btnRefreshMusicFolder_clicked();
		void on_btnSaveTags_clicked();
		void on_btnRemoveTags_clicked();
		void on_btnInsertAlbumArt_clicked();
		void on_btnFilenameToTag_clicked();
		void on_btnTagToFilename_clicked();
		void on_tblMusicFiles_itemSelectionChanged();

	private:
		//==Vars==//
		NickvisionTagger::Models::MusicFolder m_musicFolder;
		std::vector<std::shared_ptr<NickvisionTagger::Models::MusicFile>> m_selectedMusicFiles;
		//==UI==//
		Ui::TaggerPage m_ui;
	};
}
