namespace Nesquick
{
    public class Program
    {
        static void Main(string[] args)
        {
           var console = new NESConsole(new Cartridge("Roms/nestest/nestest.nes"));
           console.Run();
        }
    }
}