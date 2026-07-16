using System.IO;
using MonoMod;

namespace FortRise.Forms;

[MonoModLinkFrom("System.Windows.Forms.FileDialog")]
public abstract class FileDialog : CommonDialog
{
    public string Title 
    { 
        [MonoModLinkFrom("System.String System.Windows.Forms.FileDialog::get_Title()")]
        get; 
        [MonoModLinkFrom("System.Void System.Windows.Forms.FileDialog::set_Title(System.String)")]
        set; 
    } = string.Empty;

    public string DefaultExt 
    { 
        [MonoModLinkFrom("System.String System.Windows.Forms.FileDialog::get_DefaultExt()")]
        get; 
        [MonoModLinkFrom("System.Void System.Windows.Forms.FileDialog::set_DefaultExt(System.String)")]
        set; 
    }

    public string Filter
    { 
        [MonoModLinkFrom("System.String System.Windows.Forms.FileDialog::get_Filter()")]
        get; 
        [MonoModLinkFrom("System.Void System.Windows.Forms.FileDialog::set_Filter(System.String)")]
        set; 
    }

    public string InitialDirectory 
    { 
        [MonoModLinkFrom("System.String System.Windows.Forms.FileDialog::get_InitialDirectory()")]
        get; 
        [MonoModLinkFrom("System.Void System.Windows.Forms.FileDialog::set_InitialDirectory(System.String)")]
        set; 
    } = Directory.GetCurrentDirectory();

    public string FileName 
    { 
        [MonoModLinkFrom("System.String System.Windows.Forms.FileDialog::get_FileName()")]
        get; 
        [MonoModLinkFrom("System.Void System.Windows.Forms.FileDialog::set_FileName(System.String)")]
        set; 
    }
}

