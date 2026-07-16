using MonoMod;

namespace FortRise.Forms;

[MonoModLinkFrom("System.Windows.Forms.CommonDialog")]
public abstract class CommonDialog
{
    [MonoModLinkFrom("System.Windows.Forms.DialogResult System.Windows.Forms.CommonDialog::ShowDialog()")]
    public DialogResult ShowDialog() 
    {
        return RunDialog();
    }

    public abstract DialogResult RunDialog();
}

