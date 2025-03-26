using System;
using System.Collections.Generic;
using MonoMod;

namespace Monocle;

public class patch_Commands : Commands 
{
    private List<string> drawCommands;
    private List<string> commandHistory;
    private string currentText;
    private int seekIndex;
    private readonly char[] commandSplit = new char[2] { ' ', ','};

    public extern void orig_ctor();

    [MonoModConstructor]
    public void ctor()
    {
        orig_ctor();
        drawCommands = new List<string>(5); // let's have a capacity here so we don't crash
    }

    [MonoModReplace]
    private void EnterCommand()
    {
        string[] array = currentText.Split(commandSplit, StringSplitOptions.RemoveEmptyEntries);
        if (commandHistory.Count == 0 || commandHistory[0] != currentText)
        {
            commandHistory.Insert(0, currentText);
        }
        drawCommands.Insert(0, ">" + this.currentText);
        currentText = "";
        seekIndex = -1;
        string[] array2 = new string[array.Length - 1];
        for (int i = 1; i < array.Length; i++)
        {
            array2[i - 1] = array[i];
        }
        ExecuteCommand(array[0].ToLowerInvariant(), array2);
    }
}