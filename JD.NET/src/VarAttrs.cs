using System;

namespace JDSpace
{
    /// <summary>
    /// Variable (ScVar) attributes enum (to faster serialization).
    /// </summary>
    [Flags]
    internal enum VarAttrs : byte
    {
        /// <summary>
        /// true - CON, false - BIN or INT
        /// </summary>
        CON_TYPE = 1,
        /// <summary>
        /// true - BIN, false - INT
        /// </summary>
        BIN_TYPE = 2,
        NULL_NAME = 4,
        LB_MINUS_INF = 8,
        LB_ZERO = 16,
        UB_INF = 32,
        UB_ZERO = 64,
        NULL_VALUE = 128
    }
}