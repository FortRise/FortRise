using System;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using SDL3;


public static class XNAFileDialog
{
	// While this is unused, it is better to keep this as it somehow needs some compatibility
	public static GraphicsDevice GraphicsDevice;
	public static string Path;
	public static string StartDirectory;
	private static volatile bool isDialogOpened;

	private static unsafe void OnOpenActionDialog(IntPtr userdata, IntPtr filelist, int filter) 
    {
        if (filelist == IntPtr.Zero)
        {
			isDialogOpened = false;
            return;
        }

        if ((IntPtr)(*(byte*)filelist) == IntPtr.Zero) 
        {
			isDialogOpened = false;
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
			isDialogOpened = false;
            return;
        }

        string file = Encoding.UTF8.GetString(files[0], count);
		Path = file;
		isDialogOpened = false;
    }

	public static bool ShowDialogSynchronous(string title = null, string saveFile = null)
	{
		Path = null;
		isDialogOpened = true;

		var propID = SDL.SDL_CreateProperties();
		SDL.SDL_SetStringProperty(propID, SDL.SDL_PROP_FILE_DIALOG_TITLE_STRING, title);
		SDL.SDL_SetStringProperty(propID, SDL.SDL_PROP_FILE_DIALOG_LOCATION_STRING, StartDirectory);
		SDL.SDL_SetPointerProperty(propID, SDL.SDL_PROP_FILE_DIALOG_WINDOW_POINTER, Engine.Instance.Window.Handle);

		if (saveFile != null)
		{
			SDL.SDL_ShowFileDialogWithProperties(SDL.SDL_FileDialogType.SDL_FILEDIALOG_SAVEFILE, OnOpenActionDialog, IntPtr.Zero, propID);
		}
		else 
		{
			SDL.SDL_ShowFileDialogWithProperties(SDL.SDL_FileDialogType.SDL_FILEDIALOG_OPENFILE, OnOpenActionDialog, IntPtr.Zero, propID);
		}

		while (isDialogOpened)
		{
			SDL.SDL_PumpEvents();
		}

		SDL.SDL_DestroyProperties(propID);

		return !string.IsNullOrEmpty(Path);
	}
}