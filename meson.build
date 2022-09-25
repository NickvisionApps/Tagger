project('org.nickvision.tagger', ['cpp', 'c'], version: '2022.9.1', meson_version: '>= 0.59.0', default_options: ['warning_level=3', 'werror=false', 'cpp_std=c++20'])

gnome = import('gnome')

threads = dependency('Threads')
gtk = dependency('gtk4', version: '>= 4.6.0')
adwaita = dependency('libadwaita-1', version: '>= 1.2.0')
jsoncpp = dependency('jsoncpp')
curlpp = dependency('curlpp')
taglib = dependency('taglib')

subdir('src')

executable('org.nickvision.tagger', sources, dependencies: [threads, adwaita, jsoncpp, curlpp, taglib], install: true, install_mode: 'rwxrwxrwx')
install_data('fpcalc', install_dir: 'bin', install_mode: 'rwxrwxrwx')
install_data(resources, install_dir: 'share/icons/hicolor/scalable/apps')
install_data(resources_symbolic, install_dir: 'share/icons/hicolor/symbolic/apps')
install_data('org.nickvision.tagger.desktop', install_dir: 'share/applications')
install_data('org.nickvision.tagger.metainfo.xml', install_dir: 'share/metainfo')

gnome.post_install(gtk_update_icon_cache: true, update_desktop_database: true)