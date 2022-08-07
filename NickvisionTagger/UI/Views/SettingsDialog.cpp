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
		m_ui.separator1->setStyleSheet(ThemeHelpers::getThemedSeparatorStyle());
		ThemeHelpers::applyWin32Theme(this);
		//==Load Config==//
		m_ui.cmbTheme->setCurrentIndex(static_cast<int>(m_configuration.getTheme(false)));
		m_ui.chkIncludeSubfolders->setChecked(m_configuration.getIncludeSubfolders());
		m_ui.chkPreserveModificationTimeStamp->setChecked(m_configuration.getPreserveModificationTimeStamp());
	}

	void SettingsDialog::on_navUserInterface_clicked()
	{
		changeView(Views::UserInterface);
	}

	void SettingsDialog::on_navTagger_clicked()
	{
		changeView(Views::Tagger);
	}

	void SettingsDialog::on_btnSave_clicked()
	{
		m_configuration.setTheme(static_cast<Theme>(m_ui.cmbTheme->currentIndex()));
		m_configuration.setIncludeSubfolders(m_ui.chkIncludeSubfolders->isChecked());
		m_configuration.setPreserveModificationTimeStamp(m_ui.chkPreserveModificationTimeStamp->isChecked());
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
			m_ui.navTagger->setChecked(false);
		}
		else if (view == Views::Tagger)
		{
			m_ui.navUserInterface->setChecked(false);
			m_ui.navTagger->setChecked(true);
		}
	}
}
