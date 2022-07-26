#pragma once

#include <QWidget>
#include "ui_TaggerPage.h"

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

	private slots:
		void on_btnOpenMusicFolder_clicked();

	private:
		//==UI==//
		Ui::TaggerPage m_ui;
	};
}
