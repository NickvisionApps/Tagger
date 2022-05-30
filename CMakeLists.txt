#Project Settings
cmake_minimum_required(VERSION 3.5)
project(NickvisionTagger LANGUAGES CXX)

#CMake Flags
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_FLAGS "-Wall -Wextra")
set(CMAKE_CXX_FLAGS_DEBUG "-g")
set(CMAKE_CXX_FLAGS_RELEASE "-O1")

#Source Files
set(HELPERS helpers/curlhelpers.h helpers/curlhelpers.cpp helpers/mediahelpers.h helpers/mediahelpers.cpp)
set(UPDATE update/version.h update/version.cpp update/updateconfig.h update/updateconfig.cpp update/updater.h update/updater.cpp)
set(MODELS models/configuration.h models/configuration.cpp models/mediafiletype.h models/mediafiletype.cpp models/musicfile.h models/musicfile.cpp models/musicfolder.h models/musicfolder.cpp)
set(UI ui/application.h ui/application.cpp ui/widget.h ui/widget.cpp)
set(XML ui/controls/progressdialog.xml ui/controls/progresstracker.xml ui/controls/comboboxdialog.xml ui/views/menuhelp.xml ui/views/menutagactions.xml ui/views/mainwindow.xml ui/views/preferencesdialog.xml ui/views/shortcutsdialog.xml)
set(RESOURCES resources/org.nickvision.tagger.svg resources/org.nickvision.tagger-devel.svg resources/org.nickvision.tagger-symbolic.svg)
set(CONTROLS ui/controls/progressdialog.h ui/controls/progressdialog.cpp ui/controls/progresstracker.h ui/controls/progresstracker.cpp ui/controls/comboboxdialog.h ui/controls/comboboxdialog.cpp)
set(VIEWS ui/views/mainwindow.h ui/views/mainwindow.cpp ui/views/preferencesdialog.h ui/views/preferencesdialog.cpp ui/views/shortcutsdialog.h ui/views/shortcutsdialog.cpp)

#Create Exe
add_executable(NickvisionTagger main.cpp ${HELPERS} ${UPDATE} ${MODELS} ${UI} ${XML} ${RESOURCES} ${CONTROLS} ${VIEWS})

#Find Libraries
find_package(PkgConfig)
pkg_check_modules(GTK REQUIRED IMPORTED_TARGET gtk4)
pkg_check_modules(ADWAITA REQUIRED IMPORTED_TARGET libadwaita-1)
pkg_check_modules(JSONCPP REQUIRED IMPORTED_TARGET jsoncpp)
pkg_check_modules(CURLPP REQUIRED curlpp)
pkg_check_modules(TAGLIB REQUIRED IMPORTED_TARGET taglib)
pkg_check_modules(MUSICBRAINZ REQUIRED IMPORTED_TARGET libmusicbrainz5)

#Link Libraries
target_link_libraries(NickvisionTagger PRIVATE PkgConfig::GTK)
target_link_libraries(NickvisionTagger PRIVATE PkgConfig::ADWAITA)
target_link_libraries(NickvisionTagger PRIVATE PkgConfig::JSONCPP)
target_link_libraries(NickvisionTagger PRIVATE ${CURLPP_LDFLAGS})
target_link_libraries(NickvisionTagger PRIVATE PkgConfig::TAGLIB)
target_link_libraries(NickvisionTagger PRIVATE PkgConfig::MUSICBRAINZ)

#Generate GResources
list(APPEND CMAKE_MODULE_PATH ${CMAKE_SOURCE_DIR}/GCR_CMake/macros)
include(GlibCompileResourcesSupport)
compile_gresources(RESOURCE_FILE XML_OUT TYPE BUNDLE RESOURCES ${XML} ${RESOURCES} COMPRESS_ALL)
add_custom_target(resource ALL DEPENDS ${RESOURCE_FILE})
