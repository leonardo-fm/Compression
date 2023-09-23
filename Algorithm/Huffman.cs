using System.Text;

namespace Algorithm;

public class Huffman {
    private const int BUFFER_LENGTH = 1024; 
    private const int BYTE_BUFFER_LENGTH = 8; 
    private byte[] buffer = new byte[BUFFER_LENGTH];
    private int[] byteBuffer = new int[BYTE_BUFFER_LENGTH];
    
    public void Compress(string filePath) {
        List<Node> charList = GenerateList(filePath);
        HTree tree = GenerateTree(charList);
        GenerateCompressedFile(filePath, tree);
    }

    public void Uncompress(string filePath) {
        HTree tree = ExtractTreeFromFile(filePath);
    }
    
    private HTree ExtractTreeFromFile(string filePath) {
        
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
        using (FileStream fw = File.Create(newFilePath)) {
            // Set the dirty byte
            bool changeDirtyByte = false;
            byte[] dirtyByte = new UTF8Encoding(true).GetBytes("1");
            fw.Write(dirtyByte, 0, dirtyByte.Length);
            
            // Set the tree values
            byte[] treeInfo = new UTF8Encoding(true).GetBytes(tree.ToString() + "\n\n");
            fw.Write(treeInfo, 0, treeInfo.Length);
            
            // Set the compressed file
            int bufferIndex = 0;
            int byteBufferIndex = 0;
            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                int byteRead;
                while ((byteRead = fs.ReadByte()) != -1)
                {
                    char charValue = (char)byteRead;
                    int[] newValue = tree.GetBitsFromChar(charValue);
                    for (int i = 0; i < newValue.Length; i++) {
                        if (byteBufferIndex < BYTE_BUFFER_LENGTH) {
                            byteBuffer[byteBufferIndex] = newValue[i];
                            byteBufferIndex++;
                        } else {
                            if (bufferIndex >= BUFFER_LENGTH) {
                                fw.Write(buffer, 0, buffer.Length);
                                bufferIndex = 0;
                            }

                            buffer[bufferIndex] = (byte)GetIntValueFromByteArray(byteBuffer);
                            bufferIndex++;

                            byteBufferIndex = 0;
                        }
                    }
                }
            }
            
            if (byteBufferIndex != 0) {
                int substituteValue = byteBuffer[byteBufferIndex] == 0 ? 1 : 0;
                for (int i = byteBufferIndex + 1; i < BYTE_BUFFER_LENGTH; i++) {
                    byteBuffer[i] = substituteValue;
                }
                buffer[bufferIndex] = (byte)GetIntValueFromByteArray(byteBuffer);
                bufferIndex++;
            } else {
                changeDirtyByte = true;
            }
                
            // Buffer dump
            fw.Write(buffer, 0, bufferIndex);

            if (changeDirtyByte) {
                fw.Seek(0, SeekOrigin.Begin);
                dirtyByte = new UTF8Encoding(true).GetBytes("0");
                fw.Write(dirtyByte, 0, dirtyByte.Length);
            }
        }
    }

    private int GetIntValueFromByteArray(int[] r) {
        return r[0] * 128 +
               r[1] * 64 +
               r[2] * 32 +
               r[3] * 16 +
               r[4] * 8 +
               r[5] * 4 +
               r[6] * 2 +
               r[7] * 1;
    }
}
