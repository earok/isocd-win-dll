using System;

namespace isocd_builder {
    /// <summary>
    /// This class provides various methods for working with big and little-endian values.
    /// </summary>
    static class EndianHelper {
        /// <summary>
        /// Transforms a 4 byte unsigned int into a both endian 8 byte unsigned int.
        /// </summary>
        /// <param name="value">A 4 byte unsigned int.</param>
        /// <returns>A 8 byte both endian unsigned int.</returns>
        public static ulong BothEndian(UInt32 value) {
            ulong mask0 = 0xFF000000;
            ulong mask1 = 0x00FF0000;
            ulong mask2 = 0x0000FF00;
            ulong mask3 = 0x000000FF;

            return (ulong)value |
                   (ulong)((value & mask0) << 8) |
                   (ulong)((value & mask1) << 24) |
                   (ulong)((value & mask2) << 40) |
                   (ulong)((value & mask3) << 56);
        }

        /// <summary>
        /// Transforms a 2 byte unsigned int into a both endian 4 byte unsigned int.
        /// </summary>
        /// <param name="value">A 2 byte unsigned int.</param>
        /// <returns>A 4 byte both endian unsigned int.</returns>
        public static uint BothEndian(ushort value) {
            uint mask0 = 0xFF00;
            uint mask1 = 0x00FF;

            return (uint)value |
                   (uint)((value & mask0) << 8) |
                   (uint)((value & mask1) << 24);
        }

        /// <summary>
        /// Changes an integer's byte order (big endian->little endian || little endian->big endian).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static uint ChangeEndian(uint value) {
            uint mask0 = 0xFF000000;
            uint mask1 = 0x00FF0000;
            uint mask2 = 0x0000FF00;
            uint mask3 = 0x000000FF;

            return ((value & mask0) >> 24) |
                   ((value & mask1) >> 8) |
                   ((value & mask2) << 8) |
                   ((value & mask3) << 24);
        }

        /// <summary>
        /// Changes a word's byte order (big endian->little endian || little endian->big endian).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ushort ChangeEndian(ushort value) {
            return (ushort)((value >> 8) | (ushort)((value & 0x00FF) << 8));
        }
    }
}