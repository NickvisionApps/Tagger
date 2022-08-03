#pragma once

#include <QWidget>
#include "ui_HomePage.h"

namespace NickvisionTagger::UI::Views
{
	/// <summary>
	/// A home page for the application
	/// </summary>
	class HomePage : public QWidget
	{
		Q_OBJECT

	public:
		/// <summary>
		/// Constructs a HomePage
		/// </summary>
		/// <param name="parent">The parent of the widget, if any</param>
		HomePage(QWidget* parent = nullptr);
		/// <summary>
		/// Refreshes the theme of the page
		/// </summary>
		void refreshTheme();

	private slots:
		/// <summary>
		/// Sends the "TaggerPage.openRecentMusicFolder" message with btnRecentFolder1.text
		/// </summary>
		void on_btnRecentFolder1_clicked();
		/// <summary>
		/// Sends the "TaggerPage.openRecentMusicFolder" message with btnRecentFolder2.text
		/// </summary>
		void on_btnRecentFolder2_clicked();
		/// <summary>
		/// Sends the "TaggerPage.openRecentMusicFolder" message with btnRecentFolder3.text
		/// </summary>
		void on_btnRecentFolder3_clicked();
		/// <summary>
		/// Sends the "TaggerPage.openMusicFolder" message
		/// </summary>
		void on_btnOpenMusicFolder_clicked();
		/// <summary>
		/// Updates the alwaysStartOnHomePage configuration preference
		/// </summary>
		void on_chkAlwaysStartOnHomePage_clicked();

	private:
		//==UI==//
		Ui::HomePage m_ui;
		//==Functions==//
		/// <summary>
		/// Updates the list of recent folders
		/// </summary>
		void updateRecentFoldersList();
	};
}
