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
        HTree tree = GenerateTree(charList);
        Console.WriteLine(tree.ToString());
        GenerateUncompressedFile(filePath, tree);
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
            int fileContent = 2 + tree.GetNumberOfLeaves() * 5; //1 n dirt byte + 4 n of items (int) + n of items * 4 (int) + n of item
            fr.Seek(fileContent, SeekOrigin.Begin);
            
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
