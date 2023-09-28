using Algorithm;

class Program {
    private static string file = "ScreenToGif.exe";
    
    static void Main(string[] args) {
        HuffmanCompression cmp = new HuffmanCompression();
        cmp.Compress(@"C:\Users\Lo\Desktop\Cmp\" + file);
        Console.WriteLine("Finished compressing");
        
        HuffmanDecompression dcmp = new HuffmanDecompression();
        dcmp.Decompression(@"C:\Users\Lo\Desktop\Cmp\" + file + ".mlh");
        Console.WriteLine("Finished decompressing");
    }
}