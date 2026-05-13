using System;
using System.IO;
using UnityEngine;

public static class bl_NetworkUtility
{

    /// <summary>
    /// Writes a Vector3 to the BinaryWriter.
    /// </summary>
    /// <param name="writer">The BinaryWriter to write to.</param>
    /// <param name="vector">The Vector3 to write.</param>
    public static void WriteVector3(this BinaryWriter writer, Vector3 vector)
    {
        writer.Write(vector.x);
        writer.Write(vector.y);
        writer.Write(vector.z);
    }

    /// <summary>
    /// Reads a Vector3 from the BinaryReader.
    /// </summary>
    /// <param name="reader">The BinaryReader to read from.</param>
    /// <returns>The read Vector3.</returns>
    public static Vector3 ReadVector3(this BinaryReader reader)
    {
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Compresses a Vector3 and writes the compressed values to the BinaryWriter.
    /// </summary>
    /// <param name="writer">The BinaryWriter to write to.</param>
    /// <param name="vector">The Vector3 to compress.</param>
    /// <param name="scaleFactor">The scale factor to apply to the Vector3 components before compression. Default is 100.</param>
    public static void CompressVector3(this BinaryWriter writer, Vector3 vector, float scaleFactor = 100)
    {
        // Convert the Vector3 components to a scaled Int16 value.
        var x = (Int16)(vector.x * scaleFactor);
        var y = (Int16)(vector.y * scaleFactor);
        var z = (Int16)(vector.z * scaleFactor);

        writer.Write(x);
        writer.Write(y);
        writer.Write(z);
    }

    /// <summary>
    /// Decompresses a Vector3 by reading the compressed values from the BinaryReader.
    /// </summary>
    /// <param name="reader">The BinaryReader to read from.</param>
    /// <param name="scaleFactor">The scale factor to apply to the decompressed Vector3 components. Default is 100.</param>
    /// <returns>The decompressed Vector3.</returns>
    public static Vector3 DecompressVector3(this BinaryReader reader, float scaleFactor = 100)
    {
        Int16 x = reader.ReadInt16();
        Int16 y = reader.ReadInt16();
        Int16 z = reader.ReadInt16();

        // Convert back to Vector3 with the original scale.
        return new Vector3(x / scaleFactor, y / scaleFactor, z / scaleFactor);
    }
}
