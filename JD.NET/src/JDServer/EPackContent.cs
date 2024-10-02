using System;

namespace JDSpace
{
    /// <summary>
    /// TransferPack smart serialization head byte. It describes
    /// TransferPack content field for faster explicit serialization.
    /// </summary>
    [Flags]
    internal enum EPackContent : byte
    {
        /// <summary>
        /// Pack contains not null Model field.
        /// </summary>
        MODEL = 1,
        /// <summary>
        /// Pack contains not null LogItem field.
        /// </summary>
        LOG_ITEM = 2,
        /// <summary>
        /// Pack contains not null Data field.
        /// </summary>
        DATA = 4,
    }
}