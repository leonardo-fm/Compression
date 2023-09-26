using Algorithm;

class Program
{
    static void Main(string[] args) {
        HuffmanCompression cmp = new HuffmanCompression();
        cmp.Compress(@"C:\Users\Lo\Desktop\Cmp\Test3.txt");
        Console.WriteLine("Finished compressing");

        HuffmanDecompression dcmp = new HuffmanDecompression();
        dcmp.Decompression(@"C:\Users\Lo\Desktop\Cmp\Test3.mlh");
        Console.WriteLine("Finished uncompressing");
    }
}