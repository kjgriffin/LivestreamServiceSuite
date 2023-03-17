using SwitcherControl.BMDSwitcher;
using SwitcherControl.Safe;

namespace PiAtemTest
{
    internal class Program
    {

        static ManualResetEvent _mreTrans = new ManualResetEvent(true);
        static IBMDSwitcherManager _switcher;

        static void Main(string[] args)
        {
            Console.WriteLine("Enter IP For ATEM");
            string ip = Console.ReadLine();

            _switcher = new SafeBMDSwitcher(_mreTrans, null, "PI ATEM TEST");

            _switcher.OnSwitcherConnectionChanged += _switcher_OnSwitcherConnectionChanged;
            _switcher.SwitcherStateChanged += _switcher_SwitcherStateChanged;

            // cmd input loop
            _switcher.TryConnect(ip);



            string input = "";

            bool run = true;

            while (run)
            {
                input = Console.ReadLine();
                if (int.TryParse(input, out var num))
                {
                    if (num >= 1 && num <= 8)
                    {
                        // set a switcher preset source
                        _switcher?.PerformPresetSelect(num);
                    }
                }
                else if (input == "exit")
                {
                    run = false;
                }
            }



        }

        private static void _switcher_SwitcherStateChanged(ATEMSharedState.SwitcherState.BMDSwitcherState args)
        {
            Console.WriteLine("EVENT: state change");
        }

        private static void _switcher_OnSwitcherConnectionChanged(object? sender, bool e)
        {
            if (e)
            {
                Console.WriteLine("Switcher Connected");
            }
            else
            {
                Console.WriteLine("Switcher Disconnected");
            }
        }
    }
}