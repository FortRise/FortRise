using System;
using System.Text;
using System.Threading;
using Monocle;
using MonoMod;
using SDL3;

namespace FortRise.Forms;

[MonoModLinkFrom("System.Windows.Forms.OpenFileDialog")]
public class OpenFileDialog : FileDialog
{
    private static volatile bool isDialogOpened;
    private static string Path;


    public OpenFileDialog()
    {
    }


	private static unsafe void OnOpenActionDialog(IntPtr userdata, IntPtr filelist, int filter) 
    {
        if (filelist == IntPtr.Zero)
        {
			isDialogOpened = false;
            return;
        }

        if (*(byte*)filelist == IntPtr.Zero) 
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
    }

    public override DialogResult RunDialog()
    {
        Path = null;
        isDialogOpened = true;
		var propID = SDL.SDL_CreateProperties();
		SDL.SDL_SetStringProperty(propID, SDL.SDL_PROP_FILE_DIALOG_TITLE_STRING, Title);
		SDL.SDL_SetStringProperty(propID, SDL.SDL_PROP_FILE_DIALOG_LOCATION_STRING, InitialDirectory);
		SDL.SDL_SetPointerProperty(propID, SDL.SDL_PROP_FILE_DIALOG_WINDOW_POINTER, Engine.Instance.Window.Handle);

        SDL.SDL_ShowFileDialogWithProperties(SDL.SDL_FileDialogType.SDL_FILEDIALOG_OPENFILE, OnOpenActionDialog, IntPtr.Zero, propID);

		while (isDialogOpened && string.IsNullOrEmpty(Path))
		{
			SDL.SDL_PumpEvents();
            Thread.Sleep(10);
		}

        isDialogOpened = false;
        FileName = Path;

        Console.WriteLine(FileName);

		SDL.SDL_DestroyProperties(propID);

        if (string.IsNullOrEmpty(Path))
        {
            return DialogResult.Cancelled;
        }

        return DialogResult.Success;
    }
}

