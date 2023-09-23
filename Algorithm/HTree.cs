namespace Algorithm; 

public class HTree {
    private readonly Node root;
    private Dictionary<char, int[]> charMap;

    public HTree(Node root) {
        this.root = root;
        charMap = new Dictionary<char, int[]>();
        GenerateCharDictionary(root, new List<int>());
    }

    public int[] GetBitsFromChar(char c) {
        return charMap[c];
    }
    
    public override string ToString() {
        return root.ToString();
    }

    private void GenerateCharDictionary(Node node, List<int> currentPath) {
        if (node.IsLeaf()) charMap.Add(node.value.Value, currentPath.ToArray());
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
