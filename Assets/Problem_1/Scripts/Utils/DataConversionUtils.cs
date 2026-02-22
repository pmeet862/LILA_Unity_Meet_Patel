using UnityEngine;
using System;

namespace LILA_TEST_PROBLEM_1.Utils
{
    public static class DataConversionUtils
    {
        /// <summary>
        /// Converts a Vector3 to a 6-byte compressed byte array with 2 decimal places precision.
        /// </summary>
        /// <param name="vector">The Vector3 to convert.</param>
        /// <returns>A 6-byte array representing the compressed Vector3.</returns>
        public static byte[] ConvertVector3ToCompressedByteArray(Vector3 vector)
        {
            byte[] byteArray = new byte[6]; // 3 shorts * 2 bytes each
            ConvertVector3ToCompressedByteArrayNonAlloc(vector, byteArray);
            return byteArray;
        }

        /// <summary>
        /// Converts a Vector3 to a 6-byte compressed format and stores it in the provided byte array
        /// without allocating memory. Precision is limited to 2 decimal places.
        /// </summary>
        /// <param name="vector">The Vector3 to convert.</param>
        /// <param name="byteArray">The buffer to store the compressed data in. Must be exactly 6 bytes long.</param>
        /// <exception cref="ArgumentException">Thrown when byteArray is null or not exactly 6 bytes long.</exception>
        public static void ConvertVector3ToCompressedByteArrayNonAlloc(Vector3 vector, byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length != 6)
            {
                throw new ArgumentException("Byte array must not be null and must be exactly 6 bytes long to convert Vector3.");
            }

            // Multiply by 100 to preserve two decimal places and convert to short (16-bit integer)
            short x = (short)(vector.x * 100);
            short y = (short)(vector.y * 100);
            short z = (short)(vector.z * 100);

            byteArray[0] = (byte)(x & 0xFF); // Lower byte of x
            byteArray[1] = (byte)((x >> 8) & 0xFF); // Upper byte of x
            byteArray[2] = (byte)(y & 0xFF); // Lower byte of y
            byteArray[3] = (byte)((y >> 8) & 0xFF); // Upper byte of y
            byteArray[4] = (byte)(z & 0xFF); // Lower byte of z
            byteArray[5] = (byte)((z >> 8) & 0xFF); // Upper byte of z           
        }

        /// <summary>
        /// Converts a 6-byte compressed byte array back to a Vector3 with 2 decimal places precision.
        /// </summary>
        /// <param name="byteArray">The 6-byte compressed array to convert. Must be exactly 6 bytes long.</param>
        /// <returns>The decompressed Vector3.</returns>
        /// <exception cref="ArgumentException">Thrown when byteArray is null or not exactly 6 bytes long.</exception>
        public static Vector3 ConvertCompressedByteArrayToVector3(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length != 6)
            {
                throw new ArgumentException("Byte array must not be null and must be exactly 6 bytes long to convert to compressed Vector3.");
            }

            short x = (short)(byteArray[0] | (byteArray[1] << 8));// Combine lower and upper bytes to form the original short value for x
            short y = (short)(byteArray[2] | (byteArray[3] << 8)); // Combine lower and upper bytes to form the original short value for y
            short z = (short)(byteArray[4] | (byteArray[5] << 8)); // Combine lower and upper bytes to form the original short value for z
            return new Vector3(x / 100f, y / 100f, z / 100f); // Divide by 100 to restore original float values
        }

        /// <summary>
        /// Converts a Vector3 to a 12-byte byte array with full float precision.
        /// </summary>
        /// <param name="vector">The Vector3 to convert.</param>
        /// <returns>A 12-byte array representing the full precision Vector3.</returns>
        public static byte[] ConvertVector3ToByteArray(Vector3 vector)
        {
            byte[] byteArray = new byte[12]; // 3 floats * 4 bytes each
            ConvertVector3ToByteArrayNonAlloc(vector, byteArray);
            return byteArray;
        }

        /// <summary>
        /// Converts a Vector3 to a 12-byte format with full float precision and stores it in the provided
        /// byte array without allocating memory.
        /// </summary>
        /// <param name="vector">The Vector3 to convert.</param>
        /// <param name="byteArray">The buffer to store the full precision data in. Must be exactly 12 bytes long.</param>
        /// <exception cref="ArgumentException">Thrown when byteArray is null or not exactly 12 bytes long.</exception>
        public static void ConvertVector3ToByteArrayNonAlloc(Vector3 vector, byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length != 12)
            {
                throw new ArgumentException("Byte array must not be null and must be exactly 12 bytes long to convert Vector3");
            }

            // Convert the floats to raw 32-bit integers so we can use bitwise operations
            int xBits = BitConverter.SingleToInt32Bits(vector.x);
            int yBits = BitConverter.SingleToInt32Bits(vector.y);
            int zBits = BitConverter.SingleToInt32Bits(vector.z);

            // PACK X (4 bytes)
            byteArray[0] = (byte)(xBits & 0xFF);         // Byte 1 (Lowest)
            byteArray[1] = (byte)((xBits >> 8) & 0xFF);  // Byte 2
            byteArray[2] = (byte)((xBits >> 16) & 0xFF); // Byte 3
            byteArray[3] = (byte)((xBits >> 24) & 0xFF); // Byte 4 (Highest)

            // PACK Y (4 bytes)
            byteArray[4] = (byte)(yBits & 0xFF);
            byteArray[5] = (byte)((yBits >> 8) & 0xFF);
            byteArray[6] = (byte)((yBits >> 16) & 0xFF);
            byteArray[7] = (byte)((yBits >> 24) & 0xFF);

            // PACK Z (4 bytes)
            byteArray[8] = (byte)(zBits & 0xFF);
            byteArray[9] = (byte)((zBits >> 8) & 0xFF);
            byteArray[10] = (byte)((zBits >> 16) & 0xFF);
            byteArray[11] = (byte)((zBits >> 24) & 0xFF);
        }

        /// <summary>
        /// Converts a 12-byte byte array back to a Vector3 with full float precision.
        /// </summary>
        /// <param name="byteArray">The 12-byte array to convert. Must be exactly 12 bytes long.</param>
        /// <returns>The full precision Vector3.</returns>
        /// <exception cref="ArgumentException">Thrown when byteArray is null or not exactly 12 bytes long.</exception>
        public static Vector3 ConvertByteArrayToVector3(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length != 12)
            {
                throw new ArgumentException("Byte array must not be null and must be exactly 12 bytes long to convert to Vector3.");
            }
            // UNPACK X: Stitch the 4 bytes back into a 32-bit integer
            int xBits = byteArray[0] | (byteArray[1] << 8) | (byteArray[2] << 16) | (byteArray[3] << 24);

            // UNPACK Y
            int yBits = byteArray[4] | (byteArray[5] << 8) | (byteArray[6] << 16) | (byteArray[7] << 24);

            // UNPACK Z
            int zBits = byteArray[8] | (byteArray[9] << 8) | (byteArray[10] << 16) | (byteArray[11] << 24);

            // Convert the raw integer bits back into actual floating-point numbers
            float x = BitConverter.Int32BitsToSingle(xBits);
            float y = BitConverter.Int32BitsToSingle(yBits);
            float z = BitConverter.Int32BitsToSingle(zBits);

            return new Vector3(x, y, z);
        }
    }
}