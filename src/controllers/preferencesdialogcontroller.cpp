#include "preferencesdialogcontroller.hpp"

using namespace NickvisionTagger::Controllers;
using namespace NickvisionTagger::Models;

PreferencesDialogController::PreferencesDialogController(Configuration& configuration) : m_configuration{ configuration }
{

}

int PreferencesDialogController::getThemeAsInt() const
{
    return static_cast<int>(m_configuration.getTheme());
}

void PreferencesDialogController::setTheme(int theme)
{
    m_configuration.setTheme(static_cast<Theme>(theme));
}

bool PreferencesDialogController::getIncludeSubfolders() const
{
    return m_configuration.getIncludeSubfolders();
}

void PreferencesDialogController::setIncludeSubfolders(bool includeSubfolders)
{
    m_configuration.setIncludeSubfolders(includeSubfolders);
}

bool PreferencesDialogController::getRememberLastOpenedFolder() const
{
    return m_configuration.getRememberLastOpenedFolder();
}

void PreferencesDialogController::setRememberLastOpenedFolder(bool rememberLastOpenedFolder)
{
    m_configuration.setRememberLastOpenedFolder(rememberLastOpenedFolder);
}

bool PreferencesDialogController::getPreserveModificationTimeStamp() const
{
    return m_configuration.getPreserveModificationTimeStamp();
}

void PreferencesDialogController::setPreserveModificationTimeStamp(bool preserveModificationTimeStamp)
{
    m_configuration.setPreserveModificationTimeStamp(preserveModificationTimeStamp);
}

bool PreferencesDialogController::getOverwriteTagWithMusicBrainz() const
{
    return m_configuration.getOverwriteTagWithMusicBrainz();
}

void PreferencesDialogController::setOverwriteTagWithMusicBrainz(bool overwriteTagWithMusicBrainz)
{
    m_configuration.setOverwriteTagWithMusicBrainz(overwriteTagWithMusicBrainz);
}

void PreferencesDialogController::saveConfiguration() const
{
    m_configuration.save();
}
