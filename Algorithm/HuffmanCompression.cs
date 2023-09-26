﻿using System.Collections;

namespace Algorithm; 

public class HuffmanCompression {
    private const int BUFFER_LENGTH_READ = 1024;
    private const int BIT_BUFFER_LENGTH = 8; 
    
    private byte[] readBufferFileToCompress = new byte[BUFFER_LENGTH_READ];
    private BitArray bitBuffer = new BitArray(BIT_BUFFER_LENGTH);
    
    public void Compress(string filePath) {
        List<Node> charList = GenerateList(filePath);
        HTree tree = GenerateTree(charList);
        Console.WriteLine(tree.ToString());
        GenerateCompressedFile(filePath, tree);
    }

    private static List<Node> GenerateList(string filePath) {
        if (!File.Exists(filePath)) throw new FileNotFoundException($"The file at the path {filePath} was not founded");
        List<Node> result = new List<Node>();
        Dictionary<char, int> cd = new Dictionary<char, int>();
        using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None)) {
            int byteRead;
            while ((byteRead = fs.ReadByte()) != -1) {
                char charValue = (char)byteRead;
                if (cd.ContainsKey(charValue)) {
                    cd[charValue]++;
                } else {
                    cd.Add(charValue, 1);  
                }
            }
        }

        foreach (KeyValuePair<char, int> kvp in cd)
            result.Add(new Node(kvp.Value, kvp.Key));
        
        return result.OrderBy(x => x.value).OrderBy(x => x.compareValue).ToList();
    }

    private static HTree GenerateTree(List<Node> objs) {
        if (objs.Count > 255) throw new ArgumentOutOfRangeException("Max value for number of char is 255 chars");
        byte nOfChars = (byte)objs.Count;
        while (objs.Count > 1) {
            Node lNode = objs[0];
            Node rNode = objs[1];
            objs.RemoveRange(0, 2);
            objs.Insert(0, FuseNodes(lNode, rNode));
            
            // Bubble up
            int i = 0;
            while (i < objs.Count - 1) {
                if (objs[i].compareValue <= objs[i + 1].compareValue) break;
                Swap(i, i + 1, objs);
                i++;
            }
        }
        
        return new HTree(objs[0], nOfChars);
    }

    private static Node FuseNodes(Node lNode, Node rNode) {
        Node father = new Node(lNode.compareValue + rNode.compareValue, null);
        father.lNode = lNode;
        father.rNode = rNode;
        return father;
    }

    private static void Swap(int i1, int i2, IList<Node> objs) {
        // ReSharper disable once SwapViaDeconstruction
        Node tmp = objs[i1];
        objs[i1] = objs[i2];
        objs[i2] = tmp;
    }
    
    /// <summary>
    /// The file is compose as:
    /// 1° byte is the number of dirty bits,
    /// 2° byte is the number of char in the text,
    /// 3° to X bytes are for the tree values as nOfOccurrences*N...charValue*N,
    /// X to N bytes the file compressed,
    /// N to EOF probably is not a precise number of bits to make a byte so, if it is the case, put 0 or 1
    /// until we have a completed byte (i take the opposite of the last value) otherwise i do nothing
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="tree"></param>
    private void GenerateCompressedFile(string filePath, HTree tree) {
        string newFilePath = filePath.Substring(0, filePath.LastIndexOf('.')) + ".mlh";
        using (FileStream destinationStream = File.Create(newFilePath)) {
            // Set the dirty byte
            destinationStream.Write(new byte[1], 0, 1);
            
            // Set the number of occurrences
            destinationStream.Write(new byte[]{ tree.GetNumberOfLeaves() }, 0, sizeof(byte));
            
            // Set the tree values
            byte[] treeInfo = BitHelper.ConvertFromBitArrayToByteArray(tree.GetTreeValue());
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
                        bitBuffer[bitIndex++] = newValue[i];
                        if (bitIndex >= BIT_BUFFER_LENGTH) {
                            readBufferFileToCompress[readBufferIndex++] = BitHelper.ConvertBitArrayToByte(bitBuffer);
                            if (readBufferIndex >= BUFFER_LENGTH_READ) {
                                destinationStream.Write(readBufferFileToCompress, 0, readBufferFileToCompress.Length);
                                readBufferIndex = 0;
                            }
                            bitIndex = 0;
                        }
                    }
                }
            }
            
            int nOfDirtyBytes = BIT_BUFFER_LENGTH - bitIndex - 1; 
            if (bitIndex != 0) {
                bool substituteValue = !bitBuffer[bitIndex];
                for (int i = bitIndex + 1; i < BIT_BUFFER_LENGTH; i++)
                    bitBuffer[i] = substituteValue;
                readBufferFileToCompress[readBufferIndex++] = BitHelper.ConvertBitArrayToByte(bitBuffer);
            }
                
            // Buffer dump
            destinationStream.Write(readBufferFileToCompress, 0, readBufferIndex);

            if (nOfDirtyBytes != 0) {
                destinationStream.Seek(0, SeekOrigin.Begin);
                destinationStream.Write(new byte[]{ (byte)nOfDirtyBytes }, 0, 1);
            }
        }
    }
}
