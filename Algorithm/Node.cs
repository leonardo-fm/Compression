namespace Algorithm;

public class Node {

    public Node lNode = null;
    public Node rNode = null;
    public int compareValue;
    public char? value;

    public Node(int compareValue, char? value) {
        this.compareValue = compareValue;
        this.value = value;
    }

    public bool IsLeaf() {
        return value != null;
    }

    public override string ToString() {
        if (IsLeaf()) return compareValue.ToString() + value.ToString();
        string childValues = string.Empty;
        if (lNode != null) childValues += lNode.ToString();
        if (rNode != null) childValues += rNode.ToString();
        return childValues;
    }
}