#include "TaggerPage.h"

namespace NickvisionTagger::UI::Views
{
	TaggerPage::TaggerPage(QWidget* parent) : QWidget(parent)
	{
		//==UI==//
		m_ui.setupUi(this);
		m_ui.scrollTagProperties->setVisible(false);
	}

	void TaggerPage::on_btnOpenMusicFolder_clicked()
	{
		m_ui.scrollTagProperties->setVisible(!m_ui.scrollTagProperties->isVisible());
	}
}
