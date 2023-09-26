using System.Collections;

namespace Algorithm; 

public class HTree {
    private readonly Node root;
    private Dictionary<byte, bool[]> charMap;
    private Dictionary<byte, int> charRepetitionMap;
    private byte leafInTheTree;

    public HTree(Node root, byte leafInTheTree) {
        this.root = root;
        charMap = new Dictionary<byte, bool[]>();
        charRepetitionMap = new Dictionary<byte, int>();
        GenerateCharDictionaries(root, new List<bool>());
        this.leafInTheTree = leafInTheTree;
    }

    public BitArray GetBitsFromChar(char c) {
        return new BitArray(charMap[Convert.ToByte(c)]);
    }

    // If the lastVisitedNode is null then start from the root
    public char? GetCharFromBits(BitArray bitArray, ref int currentIndex, ref Node lastVisitedNode) {
        Node currentNode = root;
        if (lastVisitedNode != null) currentNode = lastVisitedNode;
        while (!currentNode.IsLeaf()) {
            if (currentIndex >= bitArray.Length) {
                lastVisitedNode = currentNode;
                return null;
            }
            
            if (bitArray[currentIndex] == false) currentNode = currentNode.lNode;
            if (bitArray[currentIndex] == true) currentNode = currentNode.rNode;
            currentIndex++;
        }

        if (lastVisitedNode != null) lastVisitedNode = null;
        return currentNode.value;
    }

    public byte GetNumberOfLeaves() {
        return leafInTheTree;
    }

    public BitArray GetTreeValue() {
        BitArray compValuesResponse = new BitArray(charRepetitionMap.Values.ToArray());
        BitArray valuesResponse = new BitArray(charRepetitionMap.Keys.ToArray());

        BitArray response = new BitArray(compValuesResponse.Length + valuesResponse.Length);
        for (int i = 0; i < compValuesResponse.Length; i++)
            response[i] = compValuesResponse[i];
        for (int i = 0; i < valuesResponse.Length; i++)
            response[i + compValuesResponse.Length] = valuesResponse[i];
        
        return response;
    }
    
    public override string ToString() {
        return root.ToString();
    }

    public void PrintDictionary() {
        foreach (var keyValuePair in charMap) {
            Console.Write($"{Convert.ToChar(keyValuePair.Key)}: ");
            foreach (bool b in keyValuePair.Value) {
                Console.Write(b ? 1 : 0);
            }
            Console.WriteLine();
        }
    }

    private void GenerateCharDictionaries(Node node, List<bool> currentPath) {
        if (node.IsLeaf()) {
            charMap.Add(Convert.ToByte(node.value.Value), currentPath.ToArray());
            charRepetitionMap.Add(Convert.ToByte(node.value.Value), node.compareValue);
        } else {
            if (node.lNode != null) {
                currentPath.Add(false);
                GenerateCharDictionaries(node.lNode, currentPath);
                currentPath.RemoveAt(currentPath.Count - 1);
            }
            if (node.rNode != null) {
                currentPath.Add(true);
                GenerateCharDictionaries(node.rNode, currentPath);
                currentPath.RemoveAt(currentPath.Count - 1);
            }
        }
    }
}
