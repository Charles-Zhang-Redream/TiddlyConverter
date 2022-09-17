namespace TiddlyConverter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine();
                return;
            }
            string jsonFile = args[0];
            string outputFile = args[1];


        }
    }
}