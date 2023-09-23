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

    private List<Node> ExtractListFromFile(string filePath) {
        List<Node> result = new List<Node>();
        Dictionary<char, int> cd = new Dictionary<char, int>();

        if (!File.Exists(filePath)) throw new FileNotFoundException($"The file at the path {filePath} was not founded");
        using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            // skip dirty byte
            fs.ReadByte();
            
            int i = 0;
            byte[] intValue = new byte[sizeof(int)];
            int byteRead;
            bool firstEOL = false;
            while ((byteRead = fs.ReadByte()) != -1)
            {
                if (firstEOL && (char)byteRead == '\n') break;
                
                if (i < sizeof(int)) {
                    if ((char)byteRead == '\n') firstEOL = true;
                    intValue[sizeof(int) - 1 - i] = (byte)byteRead;
                    i++;
                } else {
                    cd.Add((char)byteRead, BitConverter.ToInt32(intValue));
                    i = 0;
                }
            }
        }
        
        foreach (KeyValuePair<char, int> kvp in cd)
        {
            result.Add(new Node(kvp.Value, kvp.Key));
        }
        
        return result.OrderBy(x => x.compareValue).ToList();
    }
    
    private void GenerateUncompressedFile(string filePath, HTree tree) {
        string newFilePath = filePath.Substring(0, filePath.LastIndexOf('.')) + "_dec.txt";
        using (FileStream fr = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None)) {
            // Read the dirty byte
            bool hasDirtyByte = fr.ReadByte() == 1;

            // Create the decompressed file
            int bufferIndex = 0;
            int bitsIndex = 0;
            int fileIndex = 0;
            Node lastVisitedNode = null;
            using (FileStream fs = File.Create(newFilePath)) {
                int byteRead;
                while ((byteRead = fr.ReadByte()) != -1) {
                    if (bufferIndex < BUFFER_LENGTH_WRITE) {
                        wBuffer[bufferIndex] = (byte)byteRead;
                        bufferIndex++;
                    } else {
                        BitArray bitsArray = new BitArray(wBuffer);
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
                    BitArray bitsArray_2 = new BitArray(lastWBuffer);
                    bitsIndex = 0;
                    while (bitsIndex < bitsArray_2.Length) {
                        char? nextChar = tree.GetCharFromBits(bitsArray_2, ref bitsIndex, ref lastVisitedNode);
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
        
        return result.OrderBy(x => x.compareValue).ToList();
    }

    private HTree GenerateTree(List<Node> objs) {
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
        
        return new HTree(objs[0]);
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
     * 2° to X bytes are for the tree values as nOfOccursions,charValue;...\n\n
     * X to N bytes the file compressed
     * N to EOF probably is not a precise number of bytes after the compression so if it is the case i put 0 or 1
     * til we have a completed byte (i take the opposite of the last value) otherwise i do nothing and set the dirty byte to 0
     * */ 
    private void GenerateCompressedFile(string filePath, HTree tree) {
        string newFilePath = filePath.Substring(0, filePath.LastIndexOf('.')) + ".mlh";
        using (FileStream destinationStream = File.Create(newFilePath)) {
            // Set the dirty byte
            bool changeDirtyByte = false;
            byte[] dirtyByte = new UTF8Encoding(true).GetBytes("1");
            destinationStream.Write(dirtyByte, 0, dirtyByte.Length);
            
            // Set the tree values
            byte[] treeInfo = new UTF8Encoding(true).GetBytes(tree.ToString() + "\n\n");
            destinationStream.Write(treeInfo, 0, treeInfo.Length);
            
            // Set the compressed file
            int readBufferIndex = 0;
            int bitIndex = 0;
            using (FileStream sourceStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None)) {
                int byteRead;
                while ((byteRead = sourceStream.ReadByte()) != -1)
                {
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

                            readBufferFileToCompress[readBufferIndex] = (byte)GetIntValueFromByteArray(bitBuffer);
                            readBufferIndex++;

                            bitIndex = 0;
                        }
                    }
                }
            }
            
            if (bitIndex != 0) {
                bool substituteValue = !bitBuffer[bitIndex];
                for (int i = bitIndex + 1; i < BIT_BUFFER_LENGTH; i++) {
                    bitBuffer[i] = substituteValue;
                }
                readBufferFileToCompress[readBufferIndex] = (byte)GetIntValueFromByteArray(bitBuffer);
                readBufferIndex++;
            } else {
                changeDirtyByte = true;
            }
                
            // Buffer dump
            destinationStream.Write(readBufferFileToCompress, 0, readBufferIndex);

            if (changeDirtyByte) {
                destinationStream.Seek(0, SeekOrigin.Begin);
                dirtyByte = new UTF8Encoding(true).GetBytes("0");
                destinationStream.Write(dirtyByte, 0, dirtyByte.Length);
            }
        }
    }

    private int GetIntValueFromByteArray(BitArray r) {
        int result = 0;
        for (int i = 0; i < r.Length; i++) {
            if (r[i]) result += Convert.ToInt32(Math.Pow(2, r.Length - 1 - i));
        }
        return result;
    }
}
