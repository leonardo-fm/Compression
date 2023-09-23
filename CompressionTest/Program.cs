using Algorithm;

class Program
{
    static void Main(string[] args) {
        Huffman cmp = new Huffman();
        
        cmp.Compress(@"C:\Users\Lo\Desktop\Cmp\Test.txt");
        Console.WriteLine("Finished compressing");
        
        cmp.Uncompress(@"C:\Users\Lo\Desktop\Cmp\Test.mlh");
        Console.WriteLine("Finished uncompressing");
    }
}