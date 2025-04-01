using System;
using System.Text;
using SDL3;

namespace FortLauncher.IO;

public ref struct DialogFilter(ReadOnlySpan<char> name, ReadOnlySpan<char> pattern)
{
    public ReadOnlySpan<char> Name = name;
    public ReadOnlySpan<char> Pattern = pattern;
}

public ref struct Property
{
    public ReadOnlySpan<char> Title;
    public IntPtr Window;
    public DialogFilter Filter;

    public Property(ReadOnlySpan<char> title, DialogFilter filter)
    {
        Title = title;
        Filter = filter;
    }

    public Property(DialogFilter filter)
    {
        Title = ReadOnlySpan<char>.Empty;
        Filter = filter;
    }
}

public static class FileDialog 
{
    public static bool IsOpened => isOpened;
    public static string? ResultPath => resultPath;

    private static volatile bool isOpened;
    private static string? resultPath;
    private static unsafe void OnOpenActionDialog(IntPtr userdata, IntPtr filelist, int filter) 
    {
        if (filelist == IntPtr.Zero)
        {
            isOpened = false;
            return;
        }
        if (*(byte*)filelist == IntPtr.Zero) 
        {
            isOpened = false;
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
            isOpened = false;
            return;
        }

        string file = Encoding.UTF8.GetString(files[0], count);
        resultPath = file;
        isOpened = false;
    }

    public static unsafe void OpenFile(string? path = null, Property property = default) 
    {
        ShowDialog(path, property, SDL.SDL_FileDialogType.SDL_FILEDIALOG_OPENFILE);
    }

    public static unsafe void Save(string? path = null, Property property = default) 
    {
        ShowDialog(path, property, SDL.SDL_FileDialogType.SDL_FILEDIALOG_SAVEFILE);
    }

    public static unsafe void OpenFolder(string? path = null) 
    {
        ShowDialog(path, default, SDL.SDL_FileDialogType.SDL_FILEDIALOG_OPENFOLDER);
    }

    private static unsafe void ShowDialog(string? path = null, Property property = default, SDL.SDL_FileDialogType access = SDL.SDL_FileDialogType.SDL_FILEDIALOG_OPENFILE) 
    {
        RawString name = default;
        RawString pattern = default;
        var properties = SDL.SDL_CreateProperties();

        if (property.Filter.Name != ReadOnlySpan<char>.Empty && access != SDL.SDL_FileDialogType.SDL_FILEDIALOG_OPENFOLDER)
        {
            name = property.Filter.Name;
            pattern = property.Filter.Pattern;
            var filterStruct = new SDL.SDL_DialogFileFilter();
            filterStruct.name = name;
            filterStruct.pattern = pattern;
            Span<SDL.SDL_DialogFileFilter> fileFilters = [filterStruct];
            fixed (SDL.SDL_DialogFileFilter* filterPtr = fileFilters)
            {
                SDL.SDL_SetPointerProperty(properties, SDL.SDL_PROP_FILE_DIALOG_FILTERS_POINTER, (nint)filterPtr);
            }
            SDL.SDL_SetNumberProperty(properties, SDL.SDL_PROP_FILE_DIALOG_NFILTERS_NUMBER, 1);
        }

        if (property.Title != ReadOnlySpan<char>.Empty)
        {
            SDL.SDL_SetStringProperty(properties, SDL.SDL_PROP_FILE_DIALOG_TITLE_STRING, property.Title.ToString());
        }

        if (property.Window != IntPtr.Zero)
        {
            SDL.SDL_SetPointerProperty(properties, SDL.SDL_PROP_FILE_DIALOG_WINDOW_POINTER, property.Window);
        }

        if (path != null)
        {
            SDL.SDL_SetStringProperty(properties, SDL.SDL_PROP_FILE_DIALOG_LOCATION_STRING, path);
        }

        isOpened = true;

        SDL.SDL_ShowFileDialogWithProperties(access, OnOpenActionDialog, IntPtr.Zero, properties);

        SDL.SDL_DestroyProperties(properties);
        name.Dispose();
        pattern.Dispose();
    }
}