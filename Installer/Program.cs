using FortRise.Installer;
using System.IO;
using System;
using System.Reflection;
using Internal;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

internal class Program 
{
    public static string Version = "1.0.0";
    private static bool waitFileDialog;
    private static string TFPath;

    private unsafe static void OpenFileCallback(IntPtr userdata, IntPtr filelist, int filter)
    {
        waitFileDialog = false;
        if (filelist == IntPtr.Zero)
        {
            return;
        }

        if ((IntPtr)(*(byte*)filelist) == IntPtr.Zero) 
        {
            return;
        }
        byte **files = (byte**)filelist;
        byte *ptr = files[0];
        int count = 0;
        while (*ptr != 0)
        {
            ptr++;
            count++;
        }

        if (count <= 0)
        {
            return;
        }

        string file = Encoding.UTF8.GetString(files[0], count);
		TFPath = file;
    }

    [STAThread]
    public static void Main(string[] args)
    {
        Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        if (args.Length == 0) 
        {
            Console.WriteLine("--patch <path/to/TowerFall>");
            Console.WriteLine("--unpatch <path/to/TowerFall>");

            unsafe 
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    try 
                    {
                        NativeMethods.SetDefaultDllDirectories(NativeMethods.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
                        NativeMethods.AddDllDirectory(Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            Environment.Is64BitProcess ? "x64" : "x86"
                        ));
                    }
                    catch 
                    {
                        NativeMethods.SetDllDirectory(Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            Environment.Is64BitProcess ? "x64" : "x86"
                        ));
                    }

                    waitFileDialog = true;
                    var ext = SDL.EncodeAsUTF8("exe");
                    var name = SDL.EncodeAsUTF8("TowerFall executable");
                    var filters = new SDL.SDL_DialogFileFilter[1] 
                    {
                        new SDL.SDL_DialogFileFilter 
                        {
                            pattern = ext,
                            name = name
                        }
                    };
                    SDL.SDL_ShowOpenFileDialog(OpenFileCallback, IntPtr.Zero, IntPtr.Zero, filters, 1, null, false);

                    while (waitFileDialog)
                    {
                        SDL.SDL_PumpEvents();
                    }

                    SDL.SDL_free((IntPtr)ext);
                    SDL.SDL_free((IntPtr)name);
                    if (TFPath == null)
                    {
                        return;
                    }
                    Console.WriteLine("Is this patch? or unpatch?");
                    Console.WriteLine("[1] Patch");
                    Console.WriteLine("[2] Unpatch");
                    int key = int.Parse(Console.ReadLine());

                    args = new string[2];
                    if (key == 1)
                    {
                        args[0] = "--patch";
                    }
                    else if (key == 2)
                    {
                        args[0] = "--unpatch";
                    }
                    args[1] = TFPath;
                }
                else 
                {
                    return;
                }
            }
        }
        if (args.Length <= 1)
        {
            return;
        }
        if (!File.Exists(Path.Combine(args[1], "TowerFall.exe")))
        {
            TFPath = args[1].Replace("TowerFall.exe", "");
            if (!File.Exists(args[1]))
            {
                Console.WriteLine("TowerFall.exe not found in this directory: " + args[1]);
                return;
            }
        }
        try
        {
            Console.WriteLine("Creating Sandbox App Domain");
            AppDomain domain = null;
            var app = new AppDomainSetup();
            app.ApplicationBase = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            app.LoaderOptimization = LoaderOptimization.SingleDomain;

            domain = AppDomain.CreateDomain(
                AppDomain.CurrentDomain.FriendlyName + " FortRise Installer",
                AppDomain.CurrentDomain.Evidence,
                app,
                AppDomain.CurrentDomain.PermissionSet
            );

            Console.WriteLine("Created " + AppDomain.CurrentDomain.FriendlyName + " FortRise Installer");

            var installer = (Installer)domain.CreateInstanceAndUnwrap(
                typeof(Installer).Assembly.FullName,
                typeof(Installer).FullName
            );
            if (args[0] == "--patch")
            {
                Console.WriteLine("Installing FortRise");
                installer.Install(TFPath);
            }
            else if (args[0] == "--unpatch")
            {
                Console.WriteLine("Uninstalling FortRise");
                installer.Uninstall(TFPath);
                return;
            }

            Console.WriteLine("Unloading Sandbox App Domain");
            AppDomain.Unload(domain);
            Console.WriteLine("Unloaded");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Console.WriteLine("Installer failed!");
        }
    }
}

// https://github.com/flibitijibibo/SDL3-CS/blob/main/SDL3/SDL3.Legacy.cs
public unsafe static class SDL
{

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_DialogFileFilter
    {
        public byte* name;
        public byte* pattern;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_DialogFileCallback(IntPtr userdata, IntPtr filelist, int filter);


    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SDL_malloc(UIntPtr size);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_free(IntPtr mem);
    public static byte* EncodeAsUTF8(string str)
    {
        if (str == null)
        {
            return (byte*) 0;
        }

        var size = (str.Length * 4) + 1;
        var buffer = (byte*) SDL_malloc((UIntPtr) size);
        fixed (char* strPtr = str)
        {
            Encoding.UTF8.GetBytes(strPtr, str.Length + 1, buffer, size);
        }

        return buffer;
    }

    public static string DecodeFromUTF8(IntPtr ptr, bool shouldFree = false)
    {
        if (ptr == IntPtr.Zero)
        {
            return null;
        }

        var end = (byte*) ptr;
        while (*end != 0)
        {
            end++;
        }

        var result = new string(
            (sbyte*) ptr,
            0,
            (int) (end - (byte*) ptr),
            System.Text.Encoding.UTF8
        );

        if (shouldFree)
        {
            SDL_free(ptr);
        }

        return result;
    }

    // Taken from https://github.com/ppy/SDL3-CS
    // C# bools are not blittable, so we need this workaround
    public struct SDLBool
    {
        private readonly byte value;

        internal const byte FALSE_VALUE = 0;
        internal const byte TRUE_VALUE = 1;

        internal SDLBool(byte value)
        {
            this.value = value;
        }

        public static implicit operator bool(SDLBool b)
        {
            return b.value != FALSE_VALUE;
        }

        public static implicit operator SDLBool(bool b)
        {
            return new SDLBool(b ? TRUE_VALUE : FALSE_VALUE);
        }

        public bool Equals(SDLBool other)
        {
            return other.value == value;
        }

        public override bool Equals(object rhs)
        {
            if (rhs is bool)
            {
                return Equals((SDLBool) (bool) rhs);
            }
            else if (rhs is SDLBool)
            {
                return Equals((SDLBool) rhs);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }

    private const string nativeLibName = "SDL3";
    [DllImport(nativeLibName, EntryPoint = "SDL_GetPlatform", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr INTERNAL_SDL_GetPlatform();
    public static string SDL_GetPlatform()
    {
        return DecodeFromUTF8(INTERNAL_SDL_GetPlatform());
    }

    [DllImport(nativeLibName, EntryPoint = "SDL_ShowOpenFileDialog", CallingConvention = CallingConvention.Cdecl)]
    private static extern void INTERNAL_SDL_ShowOpenFileDialog(SDL_DialogFileCallback callback, IntPtr userdata, IntPtr window, SDL_DialogFileFilter[] filters, int nfilters, byte* default_location, SDLBool allow_many);
    public static void SDL_ShowOpenFileDialog(SDL_DialogFileCallback callback, IntPtr userdata, IntPtr window, SDL_DialogFileFilter[] filters, int nfilters, string default_location, SDLBool allow_many)
    {
        var default_locationUTF8 = EncodeAsUTF8(default_location);
        INTERNAL_SDL_ShowOpenFileDialog(callback, userdata, window, filters, nfilters, default_locationUTF8, allow_many);

        SDL_free((IntPtr) default_locationUTF8);
    }


    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_PumpEvents();
}

internal static class NativeMethods 
{
    internal const int LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetDefaultDllDirectories(int directoryFlags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern void AddDllDirectory(string lpPathName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetDllDirectory(string lpPathName);
}