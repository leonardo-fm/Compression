using System.Text;

namespace Algorithm;

public class Huffman {

    public void Compress(string filePath) {
        List<Node> charList = GenerateList(filePath);
        Node tree = GenerateTree(charList);
    }

    public void Uncompress() {
        throw new NotImplementedException();
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

    private Node GenerateTree(List<Node> objs) {
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
        
        return objs[0];
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
}
