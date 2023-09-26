using System.Collections;
using System.Text;

namespace Algorithm;

public class Huffman {
    private const int BUFFER_LENGTH_READ = 1024;
    private const int BUFFER_LENGTH_WRITE = 256;
    private const int BUFFER_LENGTH_WRITE_ON_FILE = 1024;
    private const int BIT_BUFFER_LENGTH = 8; 
    private byte[] readBufferFileToCompress = new byte[BUFFER_LENGTH_READ];
    private BitArray bitBuffer = new BitArray(BIT_BUFFER_LENGTH);
    
    private byte[] wBuffer = new byte[BUFFER_LENGTH_WRITE];
    private byte[] wBufferOnFile = new byte[BUFFER_LENGTH_WRITE_ON_FILE];
    
    public void Compress(string filePath) {
        List<Node> charList = GenerateList(filePath);
        HTree tree = GenerateTree(charList);
        GenerateCompressedFile(filePath, tree);
    }

    public void Uncompress(string filePath) {
        List<Node> charList = ExtractListFromFile(filePath);
        HTree tree = GenerateTree(charList);
        GenerateUncompressedFile(filePath, tree);
    }

    private List<Node> GenerateList(string filePath) {
        List<Node> result = new List<Node>();
        Dictionary<char, int> cd = new Dictionary<char, int>();
        if (!File.Exists(filePath)) throw new FileNotFoundException($"The file at the path {filePath} was not founded");
        using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            int byteRead;
            while ((byteRead = fs.ReadByte()) != -1)
            {
                char charValue = (char)byteRead;
                if (cd.ContainsKey(charValue)) {
                    cd[charValue]++;
                } else {
                    cd.Add(charValue, 1);  
                }
            }
        }

        foreach (KeyValuePair<char, int> kvp in cd)
        {
            result.Add(new Node(kvp.Value, kvp.Key));
        }
        
        return result.OrderBy(x => x.value).OrderBy(x => x.compareValue).ToList();
    }

    private HTree GenerateTree(List<Node> objs) {
        if (objs.Count > 255) throw new ArgumentOutOfRangeException("Max value for number of char is 255 chars");
        byte nOfChars = (byte)objs.Count;
        while (objs.Count > 1) {
            Node lNode = objs[0];
            objs.RemoveAt(0);
            Node rNode = objs[0];
            objs.RemoveAt(0);
            objs.Insert(0, FuseNodes(lNode, rNode));
            int j = 0;
            while (j < objs.Count - 1) {
                if (objs[j].compareValue <= objs[j + 1].compareValue) break;
                Swap(j, j + 1, objs);
                j++;
            }
        }
        
        return new HTree(objs[0], nOfChars);
    }

    private Node FuseNodes(Node lNode, Node rNode) {
        Node father = new Node(lNode.compareValue + rNode.compareValue, null);
        father.lNode = lNode;
        father.rNode = rNode;
        return father;
    }

    private void Swap(int i1, int i2, List<Node> objs) {
        Node tmp = objs[i1];
        objs[i1] = objs[i2];
        objs[i2] = tmp;
    }

    /* The file is compose as:
     * 1° byte is the dirty byte
     * 2° byte is the number of char in the text
     * 3° to X bytes are for the tree values as nOfOccurrences*N...charValue*N
     * X to N bytes the file compressed
     * N to EOF probably is not a precise number of bytes after the compression so if it is the case i put 0 or 1
     * til we have a completed byte (i take the opposite of the last value) otherwise i do nothing and set the dirty byte to 0
     * */ 
    private void GenerateCompressedFile(string filePath, HTree tree) {
        string newFilePath = filePath.Substring(0, filePath.LastIndexOf('.')) + ".mlh";
        using (FileStream destinationStream = File.Create(newFilePath)) {
            // Set the dirty byte
            destinationStream.Write(new byte[1], 0, 1);
            
            // Set the number of occurrences
            destinationStream.Write(new byte[]{ tree.GetNumberOfLeaves() }, 0, sizeof(byte));
            
            // Set the tree values
            BitArray treeVal = tree.GetTreeValue();
            byte[] treeInfo = new byte[treeVal.Length / 8];
            treeVal.CopyTo(treeInfo, 0);
            destinationStream.Write(treeInfo, 0, treeInfo.Length);
            
            // Set the compressed file
            int readBufferIndex = 0;
            int bitIndex = 0;
            using (FileStream sourceStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None)) {
                int byteRead;
                while ((byteRead = sourceStream.ReadByte()) != -1) {
                    char charValue = (char)byteRead;
                    BitArray newValue = tree.GetBitsFromChar(charValue);
                    for (int i = 0; i < newValue.Length; i++) {
                        bitBuffer[bitIndex] = newValue[i];
                        bitIndex++;
                        
                        if (bitIndex >= BIT_BUFFER_LENGTH) {
                            if (readBufferIndex >= BUFFER_LENGTH_READ) {
                                destinationStream.Write(readBufferFileToCompress, 0, readBufferFileToCompress.Length);
                                readBufferIndex = 0;
                            }

                            readBufferFileToCompress[readBufferIndex] = (byte)GetIntValueFromBitArray(bitBuffer);
                            readBufferIndex++;

                            bitIndex = 0;
                        }
                    }
                }
            }
            
            int nOfDirtyBytes = BIT_BUFFER_LENGTH - bitIndex - 1; 
            if (bitIndex != 0) {
                bool substituteValue = !bitBuffer[bitIndex];
                for (int i = bitIndex + 1; i < BIT_BUFFER_LENGTH; i++) {
                    bitBuffer[i] = substituteValue;
                }
                readBufferFileToCompress[readBufferIndex] = (byte)GetIntValueFromBitArray(bitBuffer);
                readBufferIndex++;
            }
                
            // Buffer dump
            destinationStream.Write(readBufferFileToCompress, 0, readBufferIndex);

            if (nOfDirtyBytes != 0) {
                destinationStream.Seek(0, SeekOrigin.Begin);
                destinationStream.Write(new byte[]{ (byte)nOfDirtyBytes }, 0, 1);
            }
        }
    }

    private int GetIntValueFromBitArray(BitArray r) {
        int result = 0;
        for (int i = 0; i < r.Length; i++) {
            if (r[i]) result += Convert.ToInt32(Math.Pow(2, i));
        }
        return result;
    }
    
    private List<Node> ExtractListFromFile(string filePath) {
        List<Node> result = new List<Node>();
        int nOfChars = 0;
        int[] compareValues;
        char[] values;
            
        if (!File.Exists(filePath)) throw new FileNotFoundException($"The file at the path {filePath} was not founded");
        using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None)) {
            // skip dirty byte
            fs.Position = 1;
            nOfChars = fs.ReadByte();
            
            compareValues = new int[nOfChars];
            values = new char[nOfChars];
            using (BinaryReader br = new BinaryReader(fs))
            {
                fs.Seek(2, SeekOrigin.Begin);
                for (int i = 0; i < nOfChars; i++)
                    compareValues[i] = br.ReadInt32();
                
                values = br.ReadChars(nOfChars);
            }
        }

        for (int i = 0; i < nOfChars; i++)
            result.Add(new Node(compareValues[i], values[i]));
        
        return result.OrderBy(x => x.value).OrderBy(x => x.compareValue).ToList();
    }
    
    private void GenerateUncompressedFile(string filePath, HTree tree) {
        string newFilePath = filePath.Substring(0, filePath.LastIndexOf('.')) + "_dec.txt";
        using (FileStream fr = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None)) {
            // Read the dirty byte
            int nOfDirtyBits = fr.ReadByte();
            int fileContent = 5 + tree.GetNumberOfLeaves() * 5; //1 n dirt byte + 4 n of items (int) + n of items * 4 (int) + n of item
            fr.Seek(fileContent - 1, SeekOrigin.Begin);
            
            // Create the decompressed file
            int bufferIndex = 0;
            int bitsIndex = 0;
            int fileIndex = 0;
            Node lastVisitedNode = null;
            BitArray bitsArray;
            using (FileStream fs = File.Create(newFilePath)) {
                int byteRead;
                while ((byteRead = fr.ReadByte()) != -1) {
                    if (bufferIndex < BUFFER_LENGTH_WRITE) {
                        wBuffer[bufferIndex] = (byte)byteRead;
                        bufferIndex++;
                    } else {
                        if (fs.Position == fs.Length) {
                            BitArray tmpBitArray = new BitArray(wBuffer);
                            bitsArray = new BitArray(wBuffer.Length * 8 - nOfDirtyBits);
                            for (int i = 0; i < bitsArray.Length; i++)
                                bitsArray[i] = tmpBitArray[i];
                        } else {
                            bitsArray = new BitArray(wBuffer);
                        }
                        
                        while (bitsIndex < bitsArray.Length) {
                            char? nextChar = tree.GetCharFromBits(bitsArray, ref bitsIndex, ref lastVisitedNode);
                            if (nextChar != null) {
                                wBufferOnFile[fileIndex] = (byte)nextChar.Value;
                                fileIndex++;
                            }
                            if (fileIndex >= wBufferOnFile.Length) {
                                fs.Write(wBufferOnFile, 0, readBufferFileToCompress.Length);
                                fileIndex = 0;
                            }
                        }
                    }
                }
                
                if (bufferIndex != 0) {
                    byte[] lastWBuffer = new byte[bufferIndex];
                    Array.Copy(wBuffer, lastWBuffer, bufferIndex);

                    if (fs.Position == fs.Length) {
                        BitArray tmpBitArray = new BitArray(lastWBuffer);
                        bitsArray = new BitArray(lastWBuffer.Length * 8 - nOfDirtyBits);
                        for (int i = 0; i < bitsArray.Length; i++)
                            bitsArray[i] = tmpBitArray[i];
                    } else {
                        bitsArray = new BitArray(lastWBuffer);
                    }
                    
                    bitsIndex = 0;
                    while (bitsIndex < bitsArray.Length) {
                        char? nextChar = tree.GetCharFromBits(bitsArray, ref bitsIndex, ref lastVisitedNode);
                        if (nextChar != null) {
                            wBufferOnFile[fileIndex] = (byte)nextChar.Value;
                            fileIndex++;
                        }
                        if (fileIndex >= wBufferOnFile.Length) {
                            fs.Write(wBufferOnFile, 0, readBufferFileToCompress.Length);
                            fileIndex = 0;
                        }
                    }
                    fs.Write(wBufferOnFile, 0, fileIndex);
                }
            }
        }
    }
}
