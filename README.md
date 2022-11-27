# Tagger
<img src="src/resources/org.nickvision.tagger.svg" width="100" height="100"/>

**An easy-to-use music tag (metadata) editor**

# Features
- Edit tags and album art of multiple files, even across subfolders, all at once
- Support for multiple music file types (mp3, ogg, flac, wma, and wav)
- Convert filenames to tags and tags to filenames with ease

# Installation
<a href='https://flathub.org/apps/details/org.nickvision.tagger'><img width='140' alt='Download on Flathub' src='https://flathub.org/assets/badges/flathub-badge-en.png'/></a>

# Chat
<a href='https://matrix.to/#/#nickvision:matrix.org'><img width='140' alt='Join our room' src='https://user-images.githubusercontent.com/17648453/196094077-c896527d-af6d-4b43-a5d8-e34a00ffd8f6.png'/></a>

# Screenshots
![MainWindow](https://user-images.githubusercontent.com/17648453/196947728-552ab0e1-6d59-4d20-b543-fb2985959958.png)
![OpenFolder](https://user-images.githubusercontent.com/17648453/196947753-9f79761f-fbec-49cc-bfb3-a9aebd1df5e2.png)
![AdvancedSearch](https://user-images.githubusercontent.com/17648453/196947764-aa1b463c-1036-4016-b902-fb4f0a41e085.png)
![TagEditing](https://user-images.githubusercontent.com/17648453/196947778-128cc2ba-3ff1-4b7f-a58c-e3edc34d1552.png)
![DarkMode](https://user-images.githubusercontent.com/17648453/196947805-2c69cf21-f071-42e6-a25c-51d4d719f6b2.png)

# Translating
Everyone is welcome to translate this app into their native or known languages, so that the application is accessible to everyone.

To translate the app, fork the repository and clone it locally. Make sure that `meson` is installed. Run the commands in your shell while in the directory of repository:
```bash
meson build
cd build
meson compile org.nickvision.tagger-pot
```
Or, if you are using GNOME Builder, build the app and then run in the Builder's terminal:
```bash
flatpak run --command=sh org.gnome.Builder
cd _build
meson compile org.nickvision.tagger-pot
```
This would generate a `NickvisionTagger/po/org.nickvision.tagger.pot` file, now you can use this file to translate the strings into your target language. You may use [Gtranslator](https://flathub.org/apps/details/org.gnome.Gtranslator) or [poedit](poedit.net) if you do not know how to translate manually in text itself. After translating (either through tools or directly in text editor), make sure to include the required metadata on the top of translation file (see existing files in `NickvisionTagger/po/` directory.)

One particular thing you should keep in mind is that some strings in this project are bifurcated into multiple strings to cater to responsiveness of the application, like:
```
msgid ""
"If checked, the currency symbol will be displayed on the right of a monetary "
"value."
```
You should use the same format for translated strings as well. But, because all languages do not have the same sentence structure, you may not need to follow this word-by-word, rather you should bifurcate the string in about the same ratio. (For examples, look into translations of languages which do not have a English-like structure in `NickvisionTagger/po/`)

Put your translated file in `NickvisionTagger/po` directory in format `<LANG>.po` where `<LANG>` is the language code.

Put the language code of your language in `NickvisionTagger/po/LINGUAS` (this file, as a convention, should remain in alphabetical order.)

Add information in `NickvisionTagger/po/CREDITS.json` so your name will appear in the app's About dialog:
```
"Jango Fett": {
    "lang": "Mandalorian",
    "email": "jango@galaxyfarfar.away"
}
```
If you made multiple translations, use an array to list all languages:
```
"C-3PO": {
    "lang": ["Ewokese", "Wookieespeak", "Jawaese"],
    "url": "https://free.droids"
}
```

To test your translation in GNOME Builder, press Ctrl+Alt+T to open a terminal inside the app's environment and then run:
```
LC_ALL=<LOCALE> /app/bin/org.nickvision.tagger
```
where `<LOCALE>` is your locale (e.g. `it_IT.UTF-8`.)

Commit these changes, and then create a pull request to the project.

As more strings may be added in the application in future, the following command needs to be ran to update all the `.po` files, which would add new strings to be translated without altering the already translated strings. But, because running this command would do this for all the languages, generally a maintainer would do that.

```bash
meson compile org.nickvision.tagger-update-po
```

# Dependencies
- [C++20](https://en.cppreference.com/w/cpp/20)
- [GTK 4](https://www.gtk.org/)
- [libadwaita](https://gnome.pages.gitlab.gnome.org/libadwaita/)
- [jsoncpp](https://github.com/open-source-parsers/jsoncpp)
- [curlpp](http://www.curlpp.org/)
- [taglib](https://taglib.org/)

# Special Thanks
- [daudix-UFO](https://github.com/daudix-UFO) and [jannuary](https://github.com/jannuary) for our application icons


