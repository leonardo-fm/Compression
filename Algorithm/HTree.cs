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
}