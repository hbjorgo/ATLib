using HeboTech.ATLib.Dtos;

namespace HeboTech.ATLib.DTOs
{
    public class PreferredMessageStorage
    {
        public PreferredMessageStorage(MessageStorage storageName, int storageMessages, int storageMessageLocations)
        {
            StorageName = storageName;
            StorageMessages = storageMessages;
            StorageMessageLocations = storageMessageLocations;
        }

        public MessageStorage StorageName { get; }
        public int StorageMessages { get; }
        public int StorageMessageLocations { get; }

        public override string ToString()
        {
            return $"Name:{StorageName}, Messages:{StorageMessages}, Locations:{StorageMessageLocations}";
        }
    }
}
