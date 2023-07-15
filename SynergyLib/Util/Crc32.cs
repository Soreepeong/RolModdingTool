using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace SynergyLib.Util;

// stolen from: https://github.com/force-net/Crc32.NET/
/// <summary>
/// Calculate the CRC32
/// </summary>
public class Crc32 {
    public static Crc32 Brb => new(0xACABADB2u, 0xEDB88320); // todo: figure this out
    public static Crc32 CryE => new(0, 0xEDB88320);

    private readonly uint _seed;
    private readonly uint[] _table;

    public Crc32(uint initialSeed, uint polynomial) {
        _seed = initialSeed;
        _table = new uint[16 * 256];
        for (uint i = 0; i < 256; i++) {
            var res = i;
            for (var t = 0; t < 16; t++) {
                for (var k = 0; k < 8; k++) {
                    res = (res & 1) == 1 ? polynomial ^ (res >> 1) : (res >> 1);
                }

                _table[t * 256 + i] = res;
            }
        }
    }

    /// <summary>
    /// Calculate the CRC32 of the given string.
    /// </summary>
    /// <param name="value">The value to hash</param>
    /// <param name="encoding">The encoding</param>
    /// <param name="crc">The initial seed/value</param>
    /// <returns>The CRC32 of the input data</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Get(string value, Encoding encoding, uint crc) {
        var data = encoding.GetBytes(value);
        return Get(data, 0, data.Length, crc);
    }

    /// <summary>
    /// Calculate the CRC32 of the given string.
    /// </summary>
    /// <param name="value">The value to hash</param>
    /// <param name="encoding">The encoding</param>
    /// <returns>The CRC32 of the input data</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Get(string value, Encoding encoding) => Get(value, encoding, _seed);

    /// <summary>
    /// Calculate the CRC32 of the given string in UTF-8.
    /// </summary>
    /// <param name="value">The value to hash</param>
    /// <returns>The CRC32 of the input data</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Get(string value) => Get(value, Encoding.UTF8, _seed);

    /// <summary>
    /// Calculate the CRC32 of the given byte array.
    /// </summary>
    /// <param name="buffer">The value to hash</param>
    /// <param name="crc">The initial seed/value</param>
    /// <returns>The CRC32 of the input data</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Get(byte[] buffer, uint crc) => Get(buffer, 0, buffer.Length, crc);

    /// <summary>
    /// Calculate the CRC32 of the given byte array.
    /// </summary>
    /// <param name="buffer">The value to hash</param>
    /// <returns>The CRC32 of the input data</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Get(byte[] buffer) => Get(buffer, _seed);

    /// <summary>
    /// Calculate the CRC32 of the given span.
    /// </summary>
    /// <param name="buffer">The value to hash</param>
    /// <param name="crc">The initial seed/value</param>
    /// <returns>The CRC32 of the input data</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Get(ReadOnlySpan<byte> buffer, uint crc) => Get(buffer, 0, buffer.Length, crc);

    /// <summary>
    /// Calculate the CRC32 of the given span.
    /// </summary>
    /// <param name="buffer">The value to hash</param>
    /// <returns>The CRC32 of the input data</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Get(ReadOnlySpan<byte> buffer) => Get(buffer, _seed);

    /// <summary>
    /// Calculate the CRC32 of the given span.
    /// </summary>
    /// <param name="buffer">The span to hash</param>
    /// <param name="start">The start index of the span to start hashing from</param>
    /// <param name="size">How many bytes to hash</param>
    /// <param name="crc">The initial seed/value</param>
    /// <returns>The CRC32 of the input data offset by <see cref="start"/> + <see cref="size"/> bytes long</returns>
    public uint Get(ReadOnlySpan<byte> buffer, int start, int size, uint crc) {
        var crcLocal = uint.MaxValue ^ crc;

        var table = _table;
        while (size >= 16) {
            var a =
                table[(3 * 256) + buffer[start + 12]]
                ^ table[(2 * 256) + buffer[start + 13]]
                ^ table[(1 * 256) + buffer[start + 14]]
                ^ table[(0 * 256) + buffer[start + 15]];

            var b =
                table[(7 * 256) + buffer[start + 8]]
                ^ table[(6 * 256) + buffer[start + 9]]
                ^ table[(5 * 256) + buffer[start + 10]]
                ^ table[(4 * 256) + buffer[start + 11]];

            var c =
                table[(11 * 256) + buffer[start + 4]]
                ^ table[(10 * 256) + buffer[start + 5]]
                ^ table[(9 * 256) + buffer[start + 6]]
                ^ table[(8 * 256) + buffer[start + 7]];

            var d =
                table[(15 * 256) + ((byte) crcLocal ^ buffer[start])]
                ^ table[(14 * 256) + ((byte) (crcLocal >> 8) ^ buffer[start + 1])]
                ^ table[(13 * 256) + ((byte) (crcLocal >> 16) ^ buffer[start + 2])]
                ^ table[(12 * 256) + ((crcLocal >> 24) ^ buffer[start + 3])];

            crcLocal = d ^ c ^ b ^ a;
            start += 16;
            size -= 16;
        }

        while (--size >= 0)
            crcLocal = table[(byte) (crcLocal ^ buffer[start++])] ^ crcLocal >> 8;

        return crcLocal ^ uint.MaxValue;
    }

    /// <summary>
    /// Calculate the CRC32 of the given span.
    /// </summary>
    /// <param name="buffer">The span to hash</param>
    /// <param name="start">The start index of the span to start hashing from</param>
    /// <param name="size">How many bytes to hash</param>
    /// <returns>The CRC32 of the input data offset by <see cref="start"/> + <see cref="size"/> bytes long</returns>
    public uint Get(ReadOnlySpan<byte> buffer, int start, int size) => Get(buffer, start, size, _seed);
}
