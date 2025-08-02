using System;

namespace EzDualMatch.Test
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            new TestDemo_SampleCode().Run();
            new TestDemo_SampleCode_001().Run();

            Console.WriteLine("\n[Done] Please press ANY key to quit!");
            Console.ReadKey();
        }
    }
}
