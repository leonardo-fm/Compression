namespace Algorithm; 

public static class Utilities {
    
    public static HTree GenerateTree(List<Node> objs) {
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
}
