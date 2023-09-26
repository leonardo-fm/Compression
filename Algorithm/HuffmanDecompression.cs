using System.Collections;

namespace Algorithm; 

public class HuffmanDecompression {
    private const int BUFFER_LENGTH_READ = 1024;
    private const int BUFFER_LENGTH_WRITE = 256;
    private const int BUFFER_LENGTH_WRITE_ON_FILE = 1024;

    private byte[] readBufferFileToCompress = new byte[BUFFER_LENGTH_READ];
    private byte[] wBuffer = new byte[BUFFER_LENGTH_WRITE];
    private byte[] wBufferOnFile = new byte[BUFFER_LENGTH_WRITE_ON_FILE];

    public void Decompression(string filePath) {
        List<Node> charList = ExtractListFromFile(filePath);
        HTree tree = Utilities.GenerateTree(charList);
        GenerateUncompressedFile(filePath, tree);
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
    
    private void GenerateUncompressedFile(string filePath, HTree tree) {
        string newFilePath = filePath.Substring(0, filePath.LastIndexOf('.')) + "_dec.txt";
        using (FileStream fr = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None)) {
            // Read the dirty byte
            int nOfDirtyBits = fr.ReadByte();
            int nOfChars = fr.ReadByte();
            
            // (byte) dirtyByte + (byte) nOfChars + (int) nOfItems + (char) valueOfChars
            int fileContentStartIndex = 2 + tree.GetNumberOfLeaves() * 5; 
            fr.Seek(fileContentStartIndex, SeekOrigin.Begin);
            
            // Create the decompressed file
            int bufferIndex = 0;
            int bitsIndex = 0;
            int fileIndex = 0;
            Node lastVisitedNode = null;
            using (FileStream fs = File.Create(newFilePath)) {
                int byteRead;
                BitArray bitArray;
                while ((byteRead = fr.ReadByte()) != -1) {
                    if (bufferIndex < BUFFER_LENGTH_WRITE) {
                        // Sequential reading from the fie
                        wBuffer[bufferIndex++] = (byte)byteRead;
                    } else {
                        if (fs.Position == fs.Length && nOfDirtyBits > 0) {
                            // Finished reading through the file, remove the dirty bits
                            BitArray tmpBitArray = new BitArray(wBuffer);
                            bitArray = new BitArray(wBuffer.Length * 8 - nOfDirtyBits);
                            for (int i = 0; i < bitArray.Length; i++)
                                bitArray[i] = tmpBitArray[i];
                        } else {
                            bitArray = new BitArray(wBuffer);
                        }
                        
                        while (bitsIndex < bitArray.Length) {
                            // The result could be null iff the tree is in the middle of a search
                            char? nextChar = tree.GetCharFromBits(bitArray, ref bitsIndex, ref lastVisitedNode);
                            if (nextChar != null)
                                wBufferOnFile[fileIndex++] = (byte)nextChar.Value;
                            if (fileIndex >= wBufferOnFile.Length) {
                                // Write into the file
                                fs.Write(wBufferOnFile, 0, readBufferFileToCompress.Length);
                                fileIndex = 0;
                            }
                        }
                    }
                }

                // If nothing is in the buffer
                if (bufferIndex == 0) return;
                
                byte[] lastWBuffer = new byte[bufferIndex];
                Array.Copy(wBuffer, lastWBuffer, bufferIndex);

                if (fs.Position == fs.Length && nOfDirtyBits > 0) {
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
                        fs.Write(wBufferOnFile, 0, readBufferFileToCompress.Length);
                        fileIndex = 0;
                    }
                }
                
                // Buffer dump
                fs.Write(wBufferOnFile, 0, fileIndex + 1);
            }
        }
    }
}
