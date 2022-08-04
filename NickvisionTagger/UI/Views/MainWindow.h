#pragma once

#include <QDragEnterEvent>
#include <QDropEvent>
#include <QMainWindow>
#include "ui_MainWindow.h"
#include "HomePage.h"
#include "TaggerPage.h"
#include "Pages.h"
#include "../../Models/Theme.h"
#include "../../Update/Updater.h"

namespace NickvisionTagger::UI::Views
{
    /// <summary>
    /// The MainWindow
    /// </summary>
    class MainWindow : public QMainWindow
    {
        Q_OBJECT

    public:
        /// <summary>
        /// Constructs a MainWindow
        /// </summary>
        /// <param name="parent">The parent of the widget, if any</param>
        MainWindow(QWidget* parent = nullptr);

    private slots:
        /// <summary>
        /// Navigate to home page
        /// </summary>
        void on_navHome_clicked();
        /// <summary>
        /// Navigate to tagger page
        /// </summary>
        void on_navTagger_clicked();
        /// <summary>
        /// Checks for an application update
        /// </summary>
        void on_navCheckForUpdates_clicked();
        /// <summary>
        /// Opens the SettingsDialog
        /// </summary>
        void on_navSettings_clicked();
        /// <summary>
        /// Opens an AboutDialog
        /// </summary>
        void on_navAbout_clicked();

    private:
        //==Vars==//
        NickvisionTagger::Models::Theme m_currentTheme;
        NickvisionTagger::Update::Updater m_updater;
        //==UI==//
        Ui::MainWindow m_ui;
        HomePage m_homePage;
        TaggerPage m_taggerPage;
        //==Functions==//
        /// <summary>
        /// Refreshes the theme of the window
        /// </summary>
        void refreshTheme();
        /// <summary>
        /// Changes the page on the window
        /// </summary>
        /// <param name="page">The page to change to</param>
        void changePage(Pages page);
        /// <summary>
        /// Occurs when an item is dragged into the MainWindow area
        /// </summary>
        /// <param name="event"></param>
        void dragEnterEvent(QDragEnterEvent* event) override;
        /// <summary>
        /// Occurs when an item is dropped into the MainWindow area
        /// </summary>
        /// <param name="event"></param>
        void dropEvent(QDropEvent* event) override;
    };
}
