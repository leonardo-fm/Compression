using Algorithm;

class Program
{
    static void Main(string[] args) {
        Huffman cmp = new Huffman();
        cmp.Compress(@"C:\Users\Lo\Desktop\Test.txt");
        Console.WriteLine("Finished compressing");
    }
}