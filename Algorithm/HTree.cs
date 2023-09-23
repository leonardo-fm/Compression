using System.Collections;

namespace Algorithm; 

public class HTree {
    private readonly Node root;
    private Dictionary<char, BitArray> charMap;

    public HTree(Node root) {
        this.root = root;
        charMap = new Dictionary<char, BitArray>();
        GenerateCharDictionary(root, new List<int>());
    }

    public BitArray GetBitsFromChar(char c) {
        return charMap[c];
    }

    // If the lastVisitedNode is null then start from the root
    public char? GetCharFromBits(BitArray bitArray, ref int currentIndex, ref Node lastVisitedNode) {
        Node currentNode = root;
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
    
    public override string ToString() {
        return root.ToString();
    }

    private void GenerateCharDictionary(Node node, List<int> currentPath) {
        if (node.IsLeaf()) charMap.Add(node.value.Value, new BitArray(currentPath.ToArray()));
        else {
            if (node.lNode != null) {
                currentPath.Add(0);
                GenerateCharDictionary(node.lNode, currentPath);
            }
            if (node.rNode != null) {
                currentPath.Add(1);
                GenerateCharDictionary(node.rNode, currentPath);
            }
        }
    }
}
