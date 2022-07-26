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

	private:
		//==UI==//
		Ui::HomePage m_ui;
	};
}
