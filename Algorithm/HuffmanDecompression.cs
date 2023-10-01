using System.Collections;

namespace Algorithm; 

public class HuffmanDecompression {
    private const int BUFFER_LENGTH_READ = 1024;
    private const int BUFFER_LENGTH_WRITE_ON_FILE = 8192;

    private byte[] rBuffer = new byte[BUFFER_LENGTH_READ];
    private byte[] wBufferOnFile = new byte[BUFFER_LENGTH_WRITE_ON_FILE];

    public void Decompression(string filePath, string decFileName) {
        List<Node> charList = ExtractListFromFile(filePath);
        HTree tree = Utilities.GenerateTree(charList);
        GenerateUncompressedFile(filePath, tree, decFileName);
    }
    
    private static List<Node> ExtractListFromFile(string filePath) {
        if (!File.Exists(filePath)) throw new FileNotFoundException($"The file at the path {filePath} was not founded");
        List<Node> result = new List<Node>();
        int nOfChars = 0;
        int[] compareValues;
        byte[] values;
        
        using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None)) {
            // skip dirty byte
            fs.Position = 1;
            nOfChars = fs.ReadByte();
            
            compareValues = new int[nOfChars];
            values = new byte[nOfChars];
            using (BinaryReader br = new BinaryReader(fs))
            {
                fs.Seek(2, SeekOrigin.Begin);
                for (int i = 0; i < nOfChars; i++)
                    compareValues[i] = br.ReadInt32();
                for (int i = 0; i < nOfChars; i++)
                    values[i] = br.ReadByte();
            }
        }

        for (int i = 0; i < nOfChars; i++)
            result.Add(new Node(compareValues[i], Convert.ToChar(values[i])));
        
        return result.OrderBy(x => x.value).OrderBy(x => x.compareValue).ToList();
    }
    
    private void GenerateUncompressedFile(string filePath, HTree tree, string decFileName) {
        string newFilePath = filePath.Substring(0, filePath.LastIndexOf('\\')) + "\\" + decFileName + ".txt";
        using (FileStream compressedFileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None)) {
            // Read the dirty byte
            int nOfDirtyBits = compressedFileStream.ReadByte();
            int nOfChars = compressedFileStream.ReadByte();
            
            // (byte) dirtyByte + (byte) nOfChars + (int) nOfItems + (char) valueOfChars
            int fileContentStartIndex = 2 + nOfChars * 5; 
            compressedFileStream.Seek(fileContentStartIndex, SeekOrigin.Begin);
            
            // Create the decompressed file
            int bufferIndex = 0;
            int bitsIndex = 0;
            int fileIndex = 0;
            Node lastVisitedNode = null;
            using (FileStream decompressedFileStream = File.Create(newFilePath)) {
                int byteRead;
                BitArray bitArray;
                while ((byteRead = compressedFileStream.ReadByte()) != -1) {
                    rBuffer[bufferIndex++] = (byte)byteRead;
                    
                    if (bufferIndex >= BUFFER_LENGTH_READ) {
                        if (compressedFileStream.Position == compressedFileStream.Length && nOfDirtyBits > 0) {
                            // Finished reading through the file, remove the dirty bits
                            BitArray tmpBitArray = new BitArray(rBuffer);
                            bitArray = new BitArray(rBuffer.Length * 8 - nOfDirtyBits);
                            for (int i = 0; i < bitArray.Length; i++)
                                bitArray[i] = tmpBitArray[i];
                        } else {
                            bitArray = new BitArray(rBuffer);
                        }
                        bufferIndex = 0;
                        
                        while (bitsIndex < bitArray.Length) {
                            // The result could be null iff the tree is in the middle of a search
                            char? nextChar = tree.GetCharFromBits(bitArray, ref bitsIndex, ref lastVisitedNode);
                            if (nextChar != null)
                                wBufferOnFile[fileIndex++] = (byte)nextChar.Value;
                            if (fileIndex >= wBufferOnFile.Length) {
                                // Write into the file
                                decompressedFileStream.Write(wBufferOnFile, 0, wBufferOnFile.Length);
                                fileIndex = 0;
                            }
                        }
                        bitsIndex = 0;
                        Console.Write($"{Math.Round(compressedFileStream.Position / (decimal) compressedFileStream.Length * 100, 2)}%\r");
                    }
                }

                // If nothing is in the buffer
                if (bufferIndex != 0) {
                    byte[] lastWBuffer = new byte[bufferIndex];
                    Array.Copy(rBuffer, lastWBuffer, bufferIndex);

                    if (compressedFileStream.Position == compressedFileStream.Length && nOfDirtyBits > 0) {
                        // Finished reading through the file, remove the dirty bits
                        BitArray tmpBitArray = new BitArray(lastWBuffer);
                        bitArray = new BitArray(lastWBuffer.Length * 8 - nOfDirtyBits);
                        for (int i = 0; i < bitArray.Length; i++)
                            bitArray[i] = tmpBitArray[i];
                    } else {
                        bitArray = new BitArray(lastWBuffer);
                    }
                
                    bitsIndex = 0;
                    while (bitsIndex < bitArray.Length) {
                        char? nextChar = tree.GetCharFromBits(bitArray, ref bitsIndex, ref lastVisitedNode);
                        if (nextChar != null)
                            wBufferOnFile[fileIndex++] = (byte)nextChar.Value;
                        if (fileIndex >= wBufferOnFile.Length) {
                            decompressedFileStream.Write(wBufferOnFile, 0, wBufferOnFile.Length);
                            fileIndex = 0;
                        }
                    }
                }
                
                // Buffer dump
                decompressedFileStream.Write(wBufferOnFile, 0, fileIndex);
            }
        }
    }
}
