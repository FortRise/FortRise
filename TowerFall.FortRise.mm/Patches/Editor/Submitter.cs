using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using FortRise;
using Microsoft.Extensions.Logging;
using MonoMod;
using Steamworks;

namespace TowerFall.Editor;

// Some users might have a itch, Humble or GOG version of the game.
[MonoModIfFlag("Steamworks")]
public class patch_Submitter : Submitter
{
    private EResult? result;
    private ulong fileID;
    private string submitPath;
    private string previewPath;
    private EditorSubmit scene;
    private Tower tower;
    private CallResult<CreateItemResult_t> onCreateItemResult;
    private CallResult<SubmitItemUpdateResult_t> onSubmitItemUpdateResult;
    private Task task;

    public string Message
    {
        [MonoModIgnore]
        get
        {
            throw new NotImplementedException();
        }
        [MonoModIgnore]
        private set
        {
            throw new NotImplementedException();
        }
    }

    public bool Success
    {
        [MonoModIgnore]
        get
        {
            throw new NotImplementedException();
        }
        [MonoModIgnore]
        private set
        {
            throw new NotImplementedException();
        }
    }

    public bool IsUpdateSubmission
    {
        [MonoModIgnore]
        get
        {
            throw new NotImplementedException();
        }
        [MonoModIgnore]
        private set
        {
            throw new NotImplementedException();
        }
    }

    private static patch_Submitter Instance;


    public patch_Submitter(EditorSubmit scene) : base(scene)
    {
    }

    [MonoModConstructor]
    [MonoModReplace]
    [MonoModIfFlag("OS:NotWindows")]
    public void ctor(EditorSubmit scene)
    {
        Instance = this;
        this.scene = scene;
        tower = scene.Tower;
        onCreateItemResult = CallResult<CreateItemResult_t>.Create(new CallResult<CreateItemResult_t>.APIDispatchDelegate(OnCreateItemResult));

        onSubmitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create(
            new CallResult<SubmitItemUpdateResult_t>.APIDispatchDelegate(OnSubmitItemUpdateResult));

        scene.Add(new SubmitProgress(this));
        task = Task.Factory.StartNew(new Action(Submit));
    }

    [MonoModIgnore]
    private extern void Submit();

    [MonoModIgnore]
    private extern void OnCreateItemResult(CreateItemResult_t result, bool ioError);

    [MonoModIgnore]
    private extern void OnSubmitItemUpdateResult(SubmitItemUpdateResult_t result, bool ioError);

    [MonoModIgnore]
    private extern string GetError(EResult result);

    [MonoModReplace]
    private bool SetUpSubmission()
    {
        WorkshopSubmissionData workshop = tower.Workshop;
        if (workshop.ID != 0UL)
        {
            IsUpdateSubmission = true;
            fileID = workshop.ID;
        }
        else
        {
            try 
            {
                IsUpdateSubmission = false;
                Message = "REGISTERING";
                result = null;

                SteamAPICall_t steamAPICall_t = SteamUGC.CreateItem(TFGame.STEAM_ID, EWorkshopFileType.k_EWorkshopFileTypeFirst);
                onCreateItemResult.Set(steamAPICall_t);
                while (result == null)
                {
                    SteamAPI.RunCallbacks();
                    Thread.Sleep(10);
                }

                if (result != EResult.k_EResultOK)
                {
                    Message = GetError(result.Value);
                    return false;
                }
            }
            catch (Exception ex)
            {
                RiseCore.logger.LogError("ERROR: {ex}", ex);
            }

            workshop.ID = fileID;
        }

        Message = "PACKAGING";
        XmlDocument xmlDocument = tower.ToXML();
        xmlDocument.Save(tower.LastSavedFilename);
        if (Directory.Exists(EditorBase.UploadDirectory))
        {
            Directory.Delete(EditorBase.UploadDirectory, true);
        }
        submitPath = Path.Combine(EditorBase.UploadDirectory, workshop.ID.ToString());
        Directory.CreateDirectory(EditorBase.UploadDirectory);
        Directory.CreateDirectory(submitPath);
        string text = Path.Combine(submitPath, Path.GetFileName(tower.LastSavedFilename));
        xmlDocument.Save(text);
        previewPath = Path.Combine(EditorBase.UploadDirectory, "preview.png");
        scene.Preview.SavePreviewPng(previewPath);
        return true;
    }
    [MonoModIgnore]
    private extern void OnCreateItemResultWORKAROUND(uint result, ulong item);

    [MonoModReplace]
    private EResult SubmitTower()
    {
        Message = "UPLOADING";
        WorkshopSubmissionData workshop = tower.Workshop;
        UGCUpdateHandle_t ugcupdateHandle_t = SteamUGC.StartItemUpdate(TFGame.STEAM_ID, new PublishedFileId_t(fileID));
        if (!submitPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            submitPath += Path.DirectorySeparatorChar;
        }

        SteamUGC.SetItemContent(ugcupdateHandle_t, submitPath);
        SteamUGC.SetItemPreview(ugcupdateHandle_t, previewPath);
        SteamUGC.SetItemTitle(ugcupdateHandle_t, workshop.Title);
        SteamUGC.SetItemDescription(ugcupdateHandle_t, workshop.Description);
        SteamUGC.SetItemVisibility(ugcupdateHandle_t, workshop.SteamVisibility);
        SteamUGC.SetItemTags(ugcupdateHandle_t, tower.GetTags());

        result = null;
        SteamAPICall_t steamAPICall_t = SteamUGC.SubmitItemUpdate(ugcupdateHandle_t, "");
        onSubmitItemUpdateResult.Set(steamAPICall_t, null);

        while (result == null)
        {
            SteamAPI.RunCallbacks();
            Thread.Sleep(10);
        }

        EResult eresult;
        if (result != EResult.k_EResultOK)
        {
            Message = GetError(result.Value);
            eresult = result.Value;
        }
        else
        {
            eresult = EResult.k_EResultOK;
        }
        return eresult;
    }
}
