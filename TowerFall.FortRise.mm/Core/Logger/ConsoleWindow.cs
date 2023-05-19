using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace FortRise;

// Original implementation
// https://github.com/BepInEx/BepInEx/tree/master/BepInEx.Core/Console
public class ConsoleWindow 
{
    private const int STD_OUTPUT_HANDLE = -11;
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetStdHandle(int nStdHandle, IntPtr hConsoleOutput);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CreateFile(string fileName, uint desiredAccess, int shareMode, IntPtr securityAttributes, int creationDisposition, int flagsAndAttributes, IntPtr templateFile);


    public static bool IsAttached;
    public static IntPtr StdOutHandle;
    public static IntPtr ConsoleOutHandle;


    public static void Attach() 
    {
        if (IsAttached)
            return;

        if (StdOutHandle == IntPtr.Zero)
            StdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);

        var current = GetConsoleWindow();
        if (current == IntPtr.Zero) 
        {
            if (!AllocConsole()) 
            {
                var error = Marshal.GetLastWin32Error();
                if (error != 5)
                    throw new Win32Exception("AllocConsole() failed");
            }
        }

        ConsoleOutHandle = CreateFile("CONOUT$", 0x80000000 | 0x40000000, 2, IntPtr.Zero, 3, 0, IntPtr.Zero); 

        if (!SetStdHandle(STD_OUTPUT_HANDLE, ConsoleOutHandle))
            throw new Win32Exception("SetStdHandle() failed");

        if (StdOutHandle != IntPtr.Zero)
            CloseHandle(StdOutHandle);

        IsAttached = true;
    }

    public static void Detach()
    {
        if (!IsAttached)
            return;

        if (!CloseHandle(ConsoleOutHandle))
            throw new Win32Exception("CloseHandle() failed");

        ConsoleOutHandle = IntPtr.Zero;

        if (!FreeConsole())
            throw new Win32Exception("FreeConsole() failed");

        if (!SetStdHandle(STD_OUTPUT_HANDLE, StdOutHandle))
            throw new Win32Exception("SetStdHandle() failed");

        IsAttached = false;
    }
}

public class WindowConsole : IConsole
{
    public TextWriter StdOut { get; private set; }

    public TextWriter ConsoleOut { get; private set; }

    public bool ConsoleActive { get; private set; }

    public void Attach()
    {
        ConsoleWindow.Attach();

        var stdOut = ConsoleWindow.ConsoleOutHandle;
        if (stdOut == IntPtr.Zero) 
        {
            StdOut = TextWriter.Null;
            ConsoleOut = TextWriter.Null;
            return;
        }
        var safeFile = new SafeFileHandle(stdOut, false);
        var origOutStream = new FileStream(safeFile, FileAccess.Write);
        StdOut = new StreamWriter(origOutStream, System.Text.Encoding.UTF8) 
        {
            AutoFlush = true
        };

        var safeFile2 = new SafeFileHandle(stdOut, false);
        var consoleOutStream = new FileStream(safeFile2, FileAccess.Write);
        ConsoleOut = new StreamWriter(consoleOutStream, System.Text.Encoding.UTF8) 
        {
            AutoFlush = true
        };
        ConsoleActive = true;
    }

    public void Detach()
    {
        ConsoleWindow.Detach();
        Console.Out.Close();
        ConsoleOut = null;

        ConsoleActive = false;
    }

    public void Initialize(bool isActive)
    {
        ConsoleActive = isActive;

        if (ConsoleActive) 
        {
            ConsoleOut = Console.Out;
            StdOut = new StreamWriter(Console.OpenStandardOutput());
            return;
        }
        StdOut = Console.Out;
    }
}

public interface IConsole
{
    TextWriter StdOut { get; }
    TextWriter ConsoleOut { get; }

    bool ConsoleActive { get; }

    void Initialize(bool isActive);
    void Attach();
    void Detach();
}