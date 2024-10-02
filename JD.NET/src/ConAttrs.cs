using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace JDSpace
{
    /// <summary>
    /// Constraint (ScVar) attributes enum (to faster serialization)
    /// </summary>
    [Flags]
    internal enum ConAttrs : byte
    {
        /// <summary>
        /// true - less equal, false - equal
        /// </summary>
        SENSE_LE = 32,
        NULL_NAME = 64,
        ZERO_CONSTANT = 128
    }
}
