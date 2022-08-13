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
    appInfo.setVersion("2022.8.3");
    appInfo.setChangelog("- Introduced ribbon interface for commands");
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
