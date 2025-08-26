using System;
using FortRise;
using MonoMod;
using SDL3;

namespace TowerFall;

// Some users might have a itch, Humble or GOG version of the game.
[MonoModIfFlag("Steamworks")]
public class patch_CustomLevelSystem : CustomLevelSystem
{
    private int[] treasureMask;

    public Steamworks.PublishedFileId_t WorkshopID
    {
        [MonoModIgnore]
        get => default;
        [MonoModIgnore]
        private set { }
    }

    public bool WorkshopSubscribed
    {
        [MonoModIgnore]
        get => false;
        [MonoModIgnore]
        private set { }
    }

    public Steamworks.EWorkshopVote WorkshopRating
    {
        [MonoModIgnore]
        get => default;
        [MonoModIgnore]
        private set { }
    }

    private Steamworks.CallResult<Steamworks.RemoteStorageUserVoteDetails_t> ratingCallback;

    public patch_CustomLevelSystem(string file) : base(file)
    {
    }
    public extern void orig_ctor(string file);

    [MonoModConstructor]
    public void ctor(string file)
    {
        orig_ctor(file);
        // Resize so we don't get any error
        Array.Resize(ref treasureMask, treasureMask.Length + PickupsRegistry.GetAllPickups().Count + 1);
    }

    [MonoModReplace]
    public void StartWorkshopLookup(Steamworks.PublishedFileId_t fileID)
    {
        WorkshopID = fileID;
        if (fileID.m_PublishedFileId != 0UL)
        {
            uint itemState = Steamworks.SteamUGC.GetItemState(fileID);
            WorkshopSubscribed = (itemState & 1U) != 0U;
            if (SDL.SDL_GetPlatform().Equals("macOS"))
            {
                WorkshopRating = Steamworks.EWorkshopVote.k_EWorkshopVoteUnvoted;
            }
            else
            {
                ratingCallback = Steamworks.CallResult<Steamworks.RemoteStorageUserVoteDetails_t>.Create(null);
                Steamworks.SteamAPICall_t userPublishedItemVoteDetails = Steamworks.SteamRemoteStorage.GetUserPublishedItemVoteDetails(WorkshopID);
                ratingCallback.Set(userPublishedItemVoteDetails, new Steamworks.CallResult<Steamworks.RemoteStorageUserVoteDetails_t>.APIDispatchDelegate(OnRatingCheck));
            }
        }
    }

    [MonoModIgnore]
    private extern void OnRatingCheck(Steamworks.RemoteStorageUserVoteDetails_t result, bool ioError);
}
