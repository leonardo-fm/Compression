using Algorithm;

class Program {
    private static string file = "ScreenToGif.exe";
    
    static void Main(string[] args) {
        HuffmanCompression cmp = new HuffmanCompression();
        Console.WriteLine("Started compressing");
        cmp.Compress(@"C:\Users\Lo\Desktop\Cmp\" + file);
        Console.WriteLine("Finished compressing");
        
        HuffmanDecompression dcmp = new HuffmanDecompression();
        Console.WriteLine("Started decompressing");
        dcmp.Decompression(@"C:\Users\Lo\Desktop\Cmp\" + file + ".mlh");
        Console.WriteLine("Finished decompressing");
    }
}