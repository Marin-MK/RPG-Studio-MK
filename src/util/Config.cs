using NativeLibraryLoader;
using System.IO;

namespace RPGStudioMK;

internal static class Config
{
    internal static PathInfo PathInfo;

    internal static void Setup()
    {
        PathPlatformInfo windows = new PathPlatformInfo(NativeLibraryLoader.Platform.Windows);
        windows.AddPath("libsdl2", "./lib/windows/SDL2.dll");
        windows.AddPath("libz", "./lib/windows/zlib1.dll");
        windows.AddPath("libsdl2_image", "./lib/windows/SDL2_image.dll");
        windows.AddPath("libpng", "./lib/windows/libpng16-16.dll");
        if (File.Exists("lib/windows/libjpeg-9.dll")) windows.AddPath("libjpeg", "./lib/windows/libjpeg-9.dll");
        windows.AddPath("libsdl2_ttf", "./lib/windows/SDL2_ttf.dll");
        windows.AddPath("libfreetype", "./lib/windows/libfreetype-6.dll");
        windows.AddPath("bass", "./lib/windows/bass.dll");
        windows.AddPath("bass_fx", "./lib/windows/bass_fx.dll");
        windows.AddPath("bass_midi", "./lib/windows/bassmidi.dll");
        windows.AddPath("tinyfiledialogs", "./lib/windows/tinyfiledialogs64.dll");
        windows.AddPath("ruby", "./lib/windows/x64-msvcrt-ruby270.dll");
        windows.AddPath("libgmp", "./lib/windows/libgmp-10.dll");
        windows.AddPath("libssp", "./lib/windows/libssp-0.dll");
        windows.AddPath("libwinpthread", "./lib/windows/libwinpthread-1.dll");
        //windows.AddPath("libcrypto", "./lib/windows/libcrypto-1_1-x64.dll");
        //windows.AddPath("libgcc", "./lib/windows/libgcc_s_seh-1.dll");
        //windows.AddPath("libffi", "./lib/windows/libffi-7.dll");
        //windows.AddPath("libyaml", "./lib/windows/libyaml-0-2.dll");

        PathPlatformInfo linux = new PathPlatformInfo(NativeLibraryLoader.Platform.Linux);
        linux.AddPath("libsdl2", "./lib/linux/SDL2.so");
        linux.AddPath("libz", "./lib/linux/libz.so");
        linux.AddPath("libsdl2_image", "./lib/linux/SDL2_image.so");
        linux.AddPath("libpng", "./lib/linux/libpng16-16.so");
        if (File.Exists("lib/linux/libjpeg-9.so")) linux.AddPath("libjpeg", "./lib/linux/libjpeg-9.so");
        linux.AddPath("libsdl2_ttf", "./lib/linux/SDL2_ttf.so");
        linux.AddPath("libfreetype", "./lib/linux/libfreetype-6.so");
        linux.AddPath("bass", "./lib/linux/libbass.so");
        linux.AddPath("bass_fx", "./lib/linux/libbass_fx.so");
        linux.AddPath("bass_midi", "./lib/linux/libbassmidi.so");
        linux.AddPath("tinyfiledialogs", "./lib/linux/tinyfiledialogs64.so");
        linux.AddPath("ruby", "./lib/linux/libruby.so");

        PathInfo = PathInfo.Create(windows, linux);
    }
}
