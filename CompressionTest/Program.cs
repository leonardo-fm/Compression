using Algorithm;

class Program {
    private static string file = "Test2";
    
    static void Main(string[] args) {
        HuffmanCompression cmp = new HuffmanCompression();
        cmp.Compress(@"C:\Users\Lo\Desktop\Cmp\" + file + ".txt");
        Console.WriteLine("Finished compressing");
        
        HuffmanDecompression dcmp = new HuffmanDecompression();
        dcmp.Decompression(@"C:\Users\Lo\Desktop\Cmp\" + file + ".mlh");
        Console.WriteLine("Finished decompressing");
    }
}