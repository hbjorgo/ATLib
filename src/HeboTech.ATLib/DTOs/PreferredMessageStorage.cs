using HeboTech.ATLib.Dtos;

namespace HeboTech.ATLib.DTOs
{
    public class PreferredMessageStorage
    {
        /// <summary>
        /// SM: SIM card storage area
        /// ME: Modem storage area
        /// MT: All storage combined
        /// BM: Broadcast message storage area
        /// SR: Status report storage area
        /// TA: Terminal adaptor storage area
        /// </summary>
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
