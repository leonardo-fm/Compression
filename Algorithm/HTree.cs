using System.Collections;
using System.Text;

namespace Algorithm; 

public class HTree {
    private readonly Node root;
    private Dictionary<char, bool[]> charMap;
    private byte leafInTheTree;

    public HTree(Node root, byte leafInTheTree) {
        this.root = root;
        charMap = new Dictionary<char, bool[]>();
        GenerateCharDictionary(root, new List<bool>());
        this.leafInTheTree = leafInTheTree;
    }

    public BitArray GetBitsFromChar(char c) {
        return new BitArray(charMap[c]);
    }

    // If the lastVisitedNode is null then start from the root
    public char? GetCharFromBits(BitArray bitArray, ref int currentIndex, ref Node lastVisitedNode) {
        Node currentNode = root;
        if (lastVisitedNode != null) currentNode = lastVisitedNode;
        while (!currentNode.IsLeaf()) {
            if (bitArray[currentIndex] == false) currentNode = currentNode.lNode;
            if (bitArray[currentIndex] == true) currentNode = currentNode.rNode;
            currentIndex++;

            if (currentIndex >= bitArray.Length) {
                lastVisitedNode = currentNode;
                return null;
            }
        }

        if (lastVisitedNode != null) lastVisitedNode = null;
        return currentNode.value;
    }

    public byte GetNumberOfLeaves() {
        return leafInTheTree;
    }

    public BitArray GetTreeValue() {
        List<(int, char)> treeKeyValues = new List<(int, char)>();
        GoThrowAllTree(root, ref treeKeyValues);
        int[] compValues = new int[GetNumberOfLeaves()];
        char[] values = new char[GetNumberOfLeaves()];
        for (int i = 0; i < treeKeyValues.Count; i++) {
            compValues[i] = treeKeyValues[i].Item1;
            values[i] = treeKeyValues[i].Item2;
        }
        
        BitArray compValuesResponse = new BitArray(compValues);
        BitArray valuesResponse = new BitArray(Encoding.UTF8.GetBytes(new string(values)));

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

    private void GenerateCharDictionary(Node node, List<bool> currentPath) {
        if (node.IsLeaf()) charMap.Add(node.value.Value, currentPath.ToArray());
        else {
            if (node.lNode != null) {
                currentPath.Add(false);
                GenerateCharDictionary(node.lNode, currentPath);
                currentPath.Remove(false);
            }
            if (node.rNode != null) {
                currentPath.Add(true);
                GenerateCharDictionary(node.rNode, currentPath);
                currentPath.Remove(true);
            }
        }
    }

    private void GoThrowAllTree(Node currentNode, ref List<(int, char)> treeKeyValues) {
        if (currentNode.IsLeaf()) {
            treeKeyValues.Add(new (currentNode.compareValue, currentNode.value.Value));
            return;
        }
        if (currentNode.lNode != null) GoThrowAllTree(currentNode.lNode, ref treeKeyValues);
        if (currentNode.rNode != null) GoThrowAllTree(currentNode.rNode, ref treeKeyValues);
    }
}
