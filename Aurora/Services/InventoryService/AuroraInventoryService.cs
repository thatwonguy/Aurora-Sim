using System;
using System.Collections.Generic;
using System.Linq;
using Aurora.DataManager.DataModels;
using Aurora.DataManager.Repositories;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Services.Interfaces;
using InventoryFolder = Aurora.DataManager.DataModels.InventoryFolder;

namespace Aurora.Services.InventoryService
{
    /// <summary>
    /// This class will operate as a conversion layer for results from the inventory repository to the opensim types
    /// </summary>
    public class AuroraInventoryService : IInventoryService
    {
        private readonly InventoryObjectType animationType;
        private readonly InventoryObjectType bodyPartType;
        private readonly InventoryObjectType callingCardType;
        private readonly InventoryObjectType clothingType;
        private readonly InventoryObjectType gestureType;
        private readonly InventoryObjectType landmarkType;
        private readonly InventoryObjectType lostAndFoundType;
        private readonly InventoryObjectType notecardType;
        private readonly InventoryObjectType objectType;
        private readonly InventoryObjectType photoType;
        private readonly InventoryRepository repository;
        private readonly InventoryObjectType scriptType;
        private readonly InventoryObjectType soundType;
        private readonly InventoryObjectType textureType;
        private readonly InventoryObjectType trashType;

        private const string FOLDER_ANIMATION_NAME = "Animations";
        private const string FOLDER_BODY_PARTS_NAME = "Body Parts";
        private const string FOLDER_CALLING_CARDS_NAME = "Calling Cards";
        private const string FOLDER_CLOTHING_NAME = "Clothing";
        private const string FOLDER_GESTURES_NAME = "Gestures";
        private const string FOLDER_LANDMARKS_NAME = "Landmarks";
        private const string FOLDER_LOST_AND_FOUND_NAME = "Lost And Found";
        private const string FOLDER_NOTECARDS = "Notecards";
        private const string FOLDER_OBJECTS_NAME = "Objects";
        private const string FOLDER_PHOTO_ALBUM_NAME = "Photo Album";
        private const string FOLDER_ROOT_NAME = "My Inventory";
        private const string FOLDER_SCRIPTS_NAME = "Scripts";
        private const string FOLDER_SOUNDS_NAME = "Sounds";
        private const string FOLDER_TEXTURES_NAME = "Textures";
        private const string FOLDER_TRASH_NAME = "Trash";

        public AuroraInventoryService()
        {
            repository = new InventoryRepository(DataManager.DataManager.DataSessionProvider);
            animationType = repository.CreateInventoryType("ANIMATION", 1);
            bodyPartType = repository.CreateInventoryType("BODY_PART", 2);
            callingCardType = repository.CreateInventoryType("CALLING_CARD", 3);
            clothingType = repository.CreateInventoryType("CLOTHING", 4);
            gestureType = repository.CreateInventoryType("GESTURE", 5);
            landmarkType = repository.CreateInventoryType("LANDMARK", 6);
            lostAndFoundType = repository.CreateInventoryType("LOST_AND_FOUND", 7);
            notecardType = repository.CreateInventoryType("NOTECARD", 8);
            objectType = repository.CreateInventoryType("OBJECT", 9);
            photoType = repository.CreateInventoryType("PHOTO", 10);
            scriptType = repository.CreateInventoryType("SCRIPT", 11);
            soundType = repository.CreateInventoryType("SOUND", 12);
            textureType = repository.CreateInventoryType("TEXTURE", 13);
            trashType = repository.CreateInventoryType("TRASH", 14);
        }

        #region IInventoryService Members

        #region Completed

        public bool CreateUserInventory(UUID user)
        {
            bool result = false;

            InventoryFolder defaultRootFolder = repository.GetRootFolder(user);
            if (defaultRootFolder == null)
            {
                defaultRootFolder = repository.CreateFolderAndSave(user, FOLDER_ROOT_NAME, UUID.Zero);
                result = true;
            }

            IList<InventoryFolder> defaultRootFolderSubFolders = repository.GetSubfoldersWithAnyAssetPreferences(defaultRootFolder);
            EnsureFolderForPreferredTypeUnderFolder(FOLDER_ANIMATION_NAME, animationType, defaultRootFolder, defaultRootFolderSubFolders);
            EnsureFolderForPreferredTypeUnderFolder(FOLDER_BODY_PARTS_NAME, bodyPartType, defaultRootFolder, defaultRootFolderSubFolders);
            EnsureFolderForPreferredTypeUnderFolder(FOLDER_CALLING_CARDS_NAME, callingCardType, defaultRootFolder, defaultRootFolderSubFolders);
            EnsureFolderForPreferredTypeUnderFolder(FOLDER_CLOTHING_NAME, clothingType, defaultRootFolder, defaultRootFolderSubFolders);
            EnsureFolderForPreferredTypeUnderFolder(FOLDER_GESTURES_NAME, gestureType, defaultRootFolder, defaultRootFolderSubFolders);
            EnsureFolderForPreferredTypeUnderFolder(FOLDER_LANDMARKS_NAME, landmarkType, defaultRootFolder, defaultRootFolderSubFolders);
            EnsureFolderForPreferredTypeUnderFolder(FOLDER_LOST_AND_FOUND_NAME, lostAndFoundType, defaultRootFolder, defaultRootFolderSubFolders);
            EnsureFolderForPreferredTypeUnderFolder(FOLDER_NOTECARDS, notecardType, defaultRootFolder, defaultRootFolderSubFolders);
            EnsureFolderForPreferredTypeUnderFolder(FOLDER_OBJECTS_NAME, objectType, defaultRootFolder, defaultRootFolderSubFolders);
            EnsureFolderForPreferredTypeUnderFolder(FOLDER_PHOTO_ALBUM_NAME, photoType, defaultRootFolder, defaultRootFolderSubFolders);
            EnsureFolderForPreferredTypeUnderFolder(FOLDER_OBJECTS_NAME, animationType, defaultRootFolder, defaultRootFolderSubFolders);
            EnsureFolderForPreferredTypeUnderFolder(FOLDER_SCRIPTS_NAME, scriptType, defaultRootFolder, defaultRootFolderSubFolders);
            EnsureFolderForPreferredTypeUnderFolder(FOLDER_SOUNDS_NAME, soundType, defaultRootFolder, defaultRootFolderSubFolders);
            EnsureFolderForPreferredTypeUnderFolder(FOLDER_TEXTURES_NAME, textureType, defaultRootFolder, defaultRootFolderSubFolders);
            EnsureFolderForPreferredTypeUnderFolder(FOLDER_TRASH_NAME, trashType, defaultRootFolder, defaultRootFolderSubFolders);

            return result;
        }

        public InventoryCollection GetUserInventory(UUID userId)
        {
            InventoryCollection collection = new InventoryCollection();
            collection.UserID = userId;
            List<InventoryFolderBase> Folders = new List<InventoryFolderBase>();
            List<InventoryItemBase> Items = new List<InventoryItemBase>();
            foreach (InventoryFolder folder in repository.GetAllFolders())
            {
                Folders.Add(ConvertInventoryFolderToInventoryFolderBase(folder));
            }
            foreach (InventoryFolderBase folder in Folders)
            {
                Items.AddRange(GetFolderItems(userId, folder.ID));
            }
            collection.Folders = Folders;
            collection.Items = Items;

            return collection;
        }

        public bool MoveFolder(InventoryFolderBase folder)
        {
            InventoryFolder IFFolder = ConvertInventoryFolderBaseToInventoryFolder(folder);
            repository.UpdateParentFolder(IFFolder);
            return true;
        }

        public bool DeleteFolders(UUID userId, List<UUID> folderIds)
        {
            repository.RemoveFolders(userId, folderIds);
            return true;
        }

        public bool PurgeFolder(InventoryFolderBase folder)
        {
            repository.RemoveFoldersAndItems(folder);
            return false;
        }

        public List<InventoryFolderBase> GetInventorySkeleton(UUID userId)
        {
            List<InventoryFolderBase> Folders = new List<InventoryFolderBase>();
            List<InventoryFolder> folders = repository.GetAllFolders(userId);
            foreach (InventoryFolder folder in folders)
            {
                Folders.Add(ConvertInventoryFolderToInventoryFolderBase(folder));
            }
            return Folders;
        }

        public InventoryFolderBase GetRootFolder(UUID userId)
        {
            return ConvertInventoryFolderToInventoryFolderBase(repository.GetRootFolder(userId));
        }

        public bool AddFolder(InventoryFolderBase folder)
        {
            InventoryFolder IFFolder = ConvertInventoryFolderBaseToInventoryFolder(folder);
            repository.CreateFolderUnderFolderAndSave(IFFolder.Name, IFFolder.Parent, IFFolder.PreferredAssetType);
            return true;
        }

        public int GetAssetPermissions(UUID userId, UUID assetId)
        {
            InventoryFolderBase parent = GetRootFolder(userId);
            return FindAssetPerms(parent, assetId);
        }

        private int FindAssetPerms(InventoryFolderBase folder, UUID assetID)
        {
            InventoryCollection contents = GetFolderContent(folder.Owner, folder.ID);

            int perms = 0;
            foreach (InventoryItemBase item in contents.Items)
            {
                if (item.AssetID == assetID)
                    perms = (int)item.CurrentPermissions | perms;
            }

            foreach (InventoryFolderBase subfolder in contents.Folders)
                perms = perms | FindAssetPerms(subfolder, assetID);

            return perms;
        }

        public List<InventoryItemBase> GetActiveGestures(UUID userId)
        {
            IList<InventoryItem> activeGestures = repository.GetActiveInventoryItemsByType(gestureType);

            var osActiveGestures = new List<InventoryItemBase>();

            foreach (InventoryItem activeGesture in activeGestures)
            {
                osActiveGestures.Add(ConvertInventoryItemToInventoryItemBase(activeGesture));
            }

            return osActiveGestures;
        }

        #endregion

        public void GetUserInventory(UUID userId, InventoryReceiptCallback callback)
        {
        }

        public InventoryFolderBase GetFolderForType(UUID userId, AssetType type)
        {
            return new InventoryFolderBase();
        }

        public InventoryCollection GetFolderContent(UUID userId, UUID folderId)
        {
            throw new NotImplementedException();
        }

        public List<InventoryItemBase> GetFolderItems(UUID userId, UUID folderId)
        {
            return new List<InventoryItemBase>();
        }

        public bool UpdateFolder(InventoryFolderBase folder)
        {
            return AddFolder(folder);
        }

        public bool AddItem(InventoryItemBase item)
        {
            return false;
        }

        public bool UpdateItem(InventoryItemBase item)
        {
            return false;
        }

        public bool DeleteItems(UUID userId, List<UUID> itemIDs)
        {
            return false;
        }

        public InventoryItemBase GetItem(InventoryItemBase item)
        {
            return item;
        }

        public InventoryFolderBase GetFolder(InventoryFolderBase folder)
        {
            return folder;
        }

        public bool HasInventoryForUser(UUID userId)
        {
            return false;
        }

        public bool MoveItems(UUID ownerId, List<InventoryItemBase> items)
        {
            return false;
        }

        #endregion

        private void EnsureFolderForPreferredTypeUnderFolder(string folderAnimationName, InventoryObjectType inventoryObjectType, InventoryFolder defaultRootFolder, IList<InventoryFolder> defaultRootFolderSubFolders)
        {
            if (!DoesFolderExistForPreferedType(defaultRootFolderSubFolders, animationType))
            {
                repository.CreateFolderUnderFolderAndSave(FOLDER_ANIMATION_NAME, defaultRootFolder, animationType);
            }
        }

        private bool DoesFolderExistForPreferedType(IList<InventoryFolder> folders, InventoryObjectType inventoryObjectType)
        {
            return (from f in folders where f.PreferredAssetType == inventoryObjectType select f).Count() > 0;
        }

        #region Converting

        private InventoryFolderBase ConvertInventoryFolderToInventoryFolderBase(InventoryFolder folder)
        {
            InventoryFolderBase IFfolder = new InventoryFolderBase();
            IFfolder.ID = new UUID(folder.Id);
            IFfolder.Name = folder.Name;
            IFfolder.Owner = new UUID(folder.Owner);
            IFfolder.ParentID = new UUID(folder.Parent.Id);
            IFfolder.Type = (short)folder.PreferredAssetType.Type;
            IFfolder.Version = 1;
            return IFfolder;
        }

        private InventoryItemBase ConvertInventoryItemToInventoryItemBase(InventoryItem activeGesture)
        {
            var inventoryItemBase = new InventoryItemBase();
            inventoryItemBase.AssetID = activeGesture.AssetUUID;
            inventoryItemBase.AssetType = (int)activeGesture.AssetType;
            inventoryItemBase.BasePermissions = (uint)activeGesture.Permissions.BaseMask;
            inventoryItemBase.CreationDate = (int)activeGesture.CreationDate.ToFileTime();
            inventoryItemBase.CreatorId = activeGesture.CreatorID.ToString();
            inventoryItemBase.CreatorIdAsUuid = activeGesture.CreatorID;
            inventoryItemBase.CurrentPermissions = (uint)activeGesture.Permissions.OwnerMask;
            inventoryItemBase.Description = activeGesture.Description;
            inventoryItemBase.EveryOnePermissions = (uint)activeGesture.Permissions.EveryoneMask;
            inventoryItemBase.Flags = activeGesture.Flags;
            inventoryItemBase.Folder = new UUID(repository.GetParentFolder(inventoryItemBase).Id);
            inventoryItemBase.GroupID = activeGesture.GroupID;
            inventoryItemBase.GroupOwned = activeGesture.GroupOwned;
            inventoryItemBase.GroupPermissions = (uint)activeGesture.Permissions.GroupMask;
            inventoryItemBase.ID = activeGesture.UUID;
            inventoryItemBase.InvType = (int)activeGesture.InventoryType;
            inventoryItemBase.Name = activeGesture.Name;
            inventoryItemBase.NextPermissions = (uint)activeGesture.Permissions.NextOwnerMask;
            inventoryItemBase.Owner = activeGesture.OwnerID;
            inventoryItemBase.SalePrice = activeGesture.SalePrice;
            inventoryItemBase.SaleType = Convert.ToByte(activeGesture.SaleType);

            return inventoryItemBase;
        }

        private InventoryFolder ConvertInventoryFolderBaseToInventoryFolder(InventoryFolderBase folder)
        {
            InventoryFolder IFfolder = new InventoryFolder();
            IFfolder.Id = folder.ID.ToString();
            IFfolder.Name = folder.Name;
            IFfolder.Owner = folder.Owner.ToString();
            IFfolder.Parent = repository.GetParentFolder(folder);
            InventoryObjectType type = repository.GetInventoryObjectTypeByType(folder.Type);
            if (type == null)
                IFfolder.PreferredAssetType = repository.CreateInventoryType(folder.Name, folder.Type);
            else
                IFfolder.PreferredAssetType = type;
            return IFfolder;
        }

        #endregion
    }
}