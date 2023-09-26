using System.Collections;
using System.ComponentModel;

namespace Algorithm; 

public static class BitHelper {

    public static byte[] ConvertFromBitArrayToByteArray(BitArray bitArray) {
        if (bitArray.Length % 8 != 0) throw new InvalidEnumArgumentException("The length of the bitArray is not possible to convert in a full byte array");
        byte[] response = new byte[bitArray.Length / 8];
        bitArray.CopyTo(response, 0);
        return response;
    }

    public static byte ConvertBitArrayToByte(BitArray bitArray) {
        if (bitArray.Length != 8) throw new InvalidEnumArgumentException("The bitArray is not 8 bit");
        return ConvertFromBitArrayToByteArray(bitArray)[0];
    }
}
