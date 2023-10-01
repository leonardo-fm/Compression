using Algorithm;

class Program {
    private static string file = "Test6_0";
    private static string directory = @"C:\Users\Lo\Desktop\Cmp\multi_lvl\";
    
    static void Main(string[] args) {
        
        HuffmanCompression cmp = new HuffmanCompression();
        Console.WriteLine("Started compressing");
        for (int i = 0; i < 10; i++) {
            cmp.Compress(directory + $"Test6_{i}.txt", $"Test6_{i + 1}");
            Console.WriteLine();
        }
        Console.WriteLine("Finished compressing");
    }
}