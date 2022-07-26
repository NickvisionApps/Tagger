#include "SettingsDialog.h"
#include "../../Helpers/ThemeHelpers.h"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;

namespace NickvisionTagger::UI::Views
{
	SettingsDialog::SettingsDialog(QWidget* parent) : QDialog{ parent }, m_configuration{ Configuration::getInstance() }
	{
		//==UI==//
		m_ui.setupUi(this);
		//==Views==//
		changeView(Views::UserInterface);
		//==Theme==//
		if (m_configuration.getTheme() == Theme::Light)
		{
			m_ui.separator->setStyleSheet("background-color: #c4c2c2;");
		}
		else
		{
			m_ui.separator->setStyleSheet("background-color: #2b2b2b;");
		}
		ThemeHelpers::applyWin32Theme(this);
		//==Load Config==//
		m_ui.cmbTheme->setCurrentIndex(static_cast<int>(m_configuration.getTheme(false)));
	}

	void SettingsDialog::on_navUserInterface_clicked()
	{
		changeView(Views::UserInterface);
	}

	void SettingsDialog::on_navApplication_clicked()
	{
		changeView(Views::Application);
	}

	void SettingsDialog::on_btnSave_clicked()
	{
		m_configuration.setTheme(static_cast<Theme>(m_ui.cmbTheme->currentIndex()));
		m_configuration.save();
		close();
	}

	void SettingsDialog::on_btnCancel_clicked()
	{
		close();
	}

	void SettingsDialog::changeView(Views view)
	{
		m_ui.viewStack->setCurrentIndex(static_cast<int>(view));
		if (view == Views::UserInterface)
		{
			m_ui.navUserInterface->setChecked(true);
			m_ui.navApplication->setChecked(false);
		}
		else if (view == Views::Application)
		{
			m_ui.navUserInterface->setChecked(false);
			m_ui.navApplication->setChecked(true);
		}
	}
}
