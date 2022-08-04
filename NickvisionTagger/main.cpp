#include <QApplication>
#include "Models/AppInfo.h"
#include "UI/Views/MainWindow.h"

using namespace NickvisionTagger::Models;
using namespace NickvisionTagger::UI::Views;

int main(int argc, char *argv[])
{
    //==App Info==//
    AppInfo& appInfo = AppInfo::getInstance();
    appInfo.setName("Nickvision Tagger");
    appInfo.setDescription("An easy-to-use music tag (metadata) editor.");
    appInfo.setVersion("2022.8.0-dev");
    appInfo.setChangelog("- Application rewrite with C++ and Qt 6\n- Added a FileSystemWatcher to allow Tagger to watch for music folder changes on disk\n- Added \"Remove Album Art\" feature to tag editor\n- Removed \"Remember Last Opened Music Folder\" feature in favor of recent folders list\n- Removed \"Download Metadata From MusicBrainz\" feature as a new library is needed to be found");
    appInfo.setGitHubRepo("https://github.com/nlogozzo/NickvisionTagger");
    appInfo.setIssueTracker("https://github.com/nlogozzo/NickvisionTagger/issues/new");
    //==App Settings==//
    QCoreApplication::setAttribute(Qt::AA_EnableHighDpiScaling, true);
    QApplication::setStyle("fusion");
    //==Run App==//
    QApplication application{ argc, argv };
    MainWindow mainWindow;
    mainWindow.showMaximized();
    return application.exec();
}
