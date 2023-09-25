using Algorithm;

class Program
{
    static void Main(string[] args) {
        Huffman cmp = new Huffman();
        cmp.Compress(@"C:\Users\Lo\Desktop\Cmp\Test2.txt");
        Console.WriteLine("Finished compressing");

        Huffman ucmp = new Huffman();
        ucmp.Uncompress(@"C:\Users\Lo\Desktop\Cmp\Test2.mlh");
        Console.WriteLine("Finished uncompressing");
    }
}