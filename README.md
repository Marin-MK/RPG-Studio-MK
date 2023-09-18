![home_logo](https://github.com/Marin-MK/RPG-Studio-MK/assets/25814892/b000346a-a4df-4dc5-8353-8862d685dd1b)

# RPG Studio MK

Welcome to the GitHub repository for RPG Studio MK.

RPG Studio MK is a game creation program that is tailored to the creation of 2D Pokémon-style games, similar to FRLG. It is intended to replace and supersede the traditional game creation tool used, RPG Maker XP. This program is the culmination of almost 6 years of development, with its official inception being December 4th, 2017.

The program was written in C# using .NET 7, and uses Ruby 2.7 at runtime.

## License
RPG Studio MK is licensed under the [**GNU General Public License v3.0 (GPLv3)**](https://github.com/Marin-MK/RPG-Studio-MK/blob/master/LICENSE).

## Support
RPG Studio MK is guaranteed to have bugs, oversights, missing features, and other issues. [**Please report them here**](https://github.com/Marin-MK/RPG-Studio-MK/issues), and I will look at them at my earliest convenience.

You can also find us on [**Discord**](https://discord.gg/Mmt3a3Y)!

## Installation
RPG Studio MK runs on all major platforms; Windows, Linux, and macOS. It can be installed by running the installer program for your platform:
- [**Windows Installer**](https://reliccastle.com/rpg-studio-mk/rpg-studio-mk-installer-win.exe)
- [**Linux Installer**](https://reliccastle.com/rpg-studio-mk/rpg-studio-mk-installer-linux)
- [**macOS Installer**](https://reliccastle.com/rpg-studio-mk/rpg-studio-mk-installer-macos)

Future updates of RPG Studio MK will be available from within RPG Studio MK itself; if it detects an update, it will ask you to update on startup of the program. If this fails, or the program is broken, you are also able to run the installer again and update that way.

## Paths
It is important that users know all the relevant files and folders should they need it for uninstalling the program, or reporting bugs.
- **Windows**
    - Installation Folder: `C:/Program Files/MK`
    - AppData Folder: `C:/Users/USERNAME/AppData/Roaming/RPG Studio MK` or `%AppData%/RPG Studio MK`
- **Linux**
    - Installation Folder: `/usr/local/bin/MK`
    - AppData Folder: `/home/USERNAME/.rpg-studio-mk` or `~/.rpg-studio-mk`
- **macOS**
    - Installation Folder: `/Applications/MK`
    - AppData Folder: `/Users/USERNAME/Libary/Application Support/RPG Studio MK`

## Related projects
Two new projects were created to help in development and distribution of RPG Studio MK: the Visual Designer program, and the Dynamic Installer.
- [**Visual Designer**](https://github.com/Marin-MK/MK-Visual-Designer) - This tool allows the user to very easily create widgets in the style and theme of RPG Studio MK, and allows users to create comprehensive windows and widgets, and manipulate almost all of their properties.
The resulting widget or window can be saved as a PNG file, and this PNG file will have all the relevant data stored as metadata so that it can be opened in VisualDesigner again to edit it once more. It also allows users to export their work to basic C#, following the traditional popup-window code conventions.
- [**Dynamic Installer**](https://github.com/Marin-MK/DynamicInstaller) - This tool allows users to install RPG Studio MK with the click of a button, and also serves as the automatic updater. When running this program, it is automatically stored and saved for use as the automatic updater.
It will then attempt to install or update RPG Studio MK, depending on whether it was installed previously.

## Compiling from source
If you are looking to build the program yourself, you need to get the following dependencies to be able to build:
- [**NativeLibraryLoader**](https://github.com/Marin-MK/NativeLibraryLoader) by [**@Marin-MK**](https://github.com/Marin-MK)
    - Library for dynamically loading any library in a cross-platform manner
- [**rubydotnet**](https://github.com/Marin-MK/rubydotnet) by [**@Marin-MK**](https://github.com/Marin-MK)
    - Library for running Ruby code in C#
- [**odl**](https://github.com/Marin-MK/odl) by [**@Marin-MK**](https://github.com/Marin-MK)
    - Library for rendering graphics/audio in a cross-platform manner
- [**amethyst**](https://github.com/Marin-MK/amethyst) by [**@Marin-MK**](https://github.com/Marin-MK)
    - Library for implementing a comprehensive UI framework
- [**decodl**](https://github.com/Marin-MK/decodl) by [**@Marin-MK**](https://github.com/Marin-MK)
    - Library for PNG encoding/decoding
- [**Crc32.NET**](https://www.nuget.org/packages/Crc32.NET/) by [**force**](https://www.nuget.org/profiles/force)
    - Library for calculating CRC32 checksums (**decodl** dependency)
- [**MKUtils**](https://github.com/Marin-MK/MKUtils) by [**@Marin-MK**](https://github.com/Marin-MK)
    - Library for networking, versioning and logging utilities

### Dynamic Dependencies
With these dependencies all installed and in the appropriate locations, you will be able to build the program. The program has several dynamically loaded dependencies too, though.
These dependencies are located in the `lib` folder besides the executable location. Each platform has its own dependencies, located in `lib/windows`, `lib/linux`, and `lib/macos`.

The only common denominator for the dynamic dependencies are the Ruby extensions located in `lib/ruby`. These are the same for all platforms. For the associated dynamic libraries that Ruby uses, those all have their own subfolder in this `lib/ruby` folder, which will be listed below for each supported platform.

#### Windows
All dynamic dependencies on Windows are `.dll` files, located in `lib/windows`. Ruby's platform-dependent dynamic libraries are located in `lib/ruby/2.7.0/x64-mingw32`.
- [**bass.dll**](https://www.un4seen.com/)
- [**bass_fx.dll**](https://www.un4seen.com/)
- [**bassmidi.dll**](https://www.un4seen.com/)
- [**libfreetype-6.dll**](https://freetype.org/)
- [**x64-msvcrt-ruby270.dll**, **libgmp-10.dll**, **libssp-0.dll**, **libwinpthread-1.dll**](https://www.ruby-lang.org/en/) - Ruby 2.7
- [**libpng16-16.dll**](http://www.libpng.org/pub/png/libpng.html)
- [**SDL2.dll**](https://github.com/libsdl-org/SDL)
- [**SDL2_image.dll**](https://github.com/libsdl-org/SDL_image)
- [**SDL2_ttf.dll**](https://github.com/libsdl-org/SDL_ttf)
- [**tinyfiledialogs64.dll**](https://github.com/native-toolkit/libtinyfiledialogs)
- [**zlib1.dll**](https://www.zlib.net/)

#### Linux
All dynamic dependencies on Linux are `.so` files, located in `lib/linux`. Ruby's platform-dependent dynamic libraries are located in `lib/ruby/2.7.0/x86_64-linux`.
- [**libbass.so**](https://www.un4seen.com/)
- [**libbass_fx.so**](https://www.un4seen.com/)
- [**libbassmidi.so**](https://www.un4seen.com/)
- [**libfreetype-6.dll**](https://freetype.org/)
- [**libruby.so**](https://www.ruby-lang.org/en/) - Ruby 2.7
- [**libpng16-16.so**](http://www.libpng.org/pub/png/libpng.html)
- [**libz.so**](https://www.zlib.net/)
- [**SDL2.so**](https://github.com/libsdl-org/SDL)
- [**SDL2_image.so**](https://github.com/libsdl-org/SDL_image)
- [**SDL2_ttf.so**](https://github.com/libsdl-org/SDL_ttf)
- [**tinyfiledialogs64.so**](https://github.com/native-toolkit/libtinyfiledialogs)

### macOS
All dynamic dependencies on macOS are `.dylib` files, located in `lib/macos`. Ruby's platform-dependent dynamic libraries are located in `lib/ruby/2.7.0/arm64-darwin22`.
- [**libbass.dylib**](https://www.un4seen.com/)
- [**libbass_fx.dylib**](https://www.un4seen.com/)
- [**libbassmidi.dylib**](https://www.un4seen.com/)
- [**libfreetype.dylib**](https://freetype.org/)
- [**libruby.dylib**](https://www.ruby-lang.org/en/) - Ruby 2.7
- [**libpng.dylib**](http://www.libpng.org/pub/png/libpng.html)
- [**libz.dylib**](https://www.zlib.net/)
- [**SDL2.dylib**](https://github.com/libsdl-org/SDL)
- [**SDL2_image.dylib**](https://github.com/libsdl-org/SDL_image)
- [**SDL2_ttf.dylib**](https://github.com/libsdl-org/SDL_ttf)
- [**tinyfiledialogs.dylib**](https://github.com/native-toolkit/libtinyfiledialogs)
