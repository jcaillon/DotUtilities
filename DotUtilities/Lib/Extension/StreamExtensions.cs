#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (StreamExtensions.cs) is part of Oetools.Utilities.
//
// Oetools.Utilities is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Oetools.Utilities is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Oetools.Utilities. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System;
using System.IO;

namespace Oetools.Utilities.Lib.Extension {

    /// <summary>
    /// A collection of extensions to convert bytes of all length.
    /// </summary>
    public static class StreamExtensions {

        /// <summary>
        /// Returns a datetime by adding <paramref name="secondsFromDayZero"/> to 1970.
        /// </summary>
        /// <param name="secondsFromDayZero"></param>
        /// <returns></returns>
        public static DateTime GetDatetimeFromUint(this uint secondsFromDayZero) {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(secondsFromDayZero).ToLocalTime();
        }

        /// <summary>
        /// Gets the numbers of seconds between the given time and 1970.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static uint GetUintFromDateTime(this DateTime date) {
            return (uint) Math.Round((date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
        }

        /// <summary>
        /// Reads a ulong from the stream, using Big Endian byte order.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ushort ReadUInt16Be(this BinaryReader reader) {
            return BitConverter.ToUInt16(reader.ReadBytesRequired(sizeof(ushort)).ReverseIfLittleEndian(), 0);
        }

        /// <summary>
        /// Reads a uint from the stream, using Big Endian byte order.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static uint ReadUInt32Be(this BinaryReader reader) {
            return BitConverter.ToUInt32(reader.ReadBytesRequired(sizeof(uint)).ReverseIfLittleEndian(), 0);
        }

        /// <summary>
        /// Reads a uint from the stream, using Big Endian byte order.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static long ReadUInt64Be(this BinaryReader reader) {
            return BitConverter.ToInt64(reader.ReadBytesRequired(sizeof(long)).ReverseIfLittleEndian(), 0);
        }

        /// <summary>
        /// Write a ulong to the stream, using Big Endian byte order.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void WriteUInt16Be(this BinaryWriter writer, ushort value) {
            writer.Write(BitConverter.GetBytes(value).ReverseIfLittleEndian());
        }

        /// <summary>
        /// Write a uint to the stream, using Big Endian byte order.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void WriteUInt32Be(this BinaryWriter writer, uint value) {
            writer.Write(BitConverter.GetBytes(value).ReverseIfLittleEndian());
        }

        /// <summary>
        /// Write a uint to the stream, using Big Endian byte order.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void WriteUInt64Be(this BinaryWriter writer, long value) {
            writer.Write(BitConverter.GetBytes(value).ReverseIfLittleEndian());
        }

        /// <summary>
        /// Read the required amount of bytes.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"></exception>
        private static byte[] ReadBytesRequired(this BinaryReader reader, int byteCount) {
            var result = reader.ReadBytes(byteCount);
            if (result.Length != byteCount)
                throw new EndOfStreamException(string.Format("{0} bytes required from stream, but only {1} returned.", byteCount, result.Length));
            return result;
        }

        /// <summary>
        /// Reverse the byte array if needed.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private static byte[] ReverseIfLittleEndian(this byte[] b) {
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(b);
            }
            return b;
        }
    }
}
