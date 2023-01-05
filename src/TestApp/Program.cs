using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwinCAT.TypeSystem;
using TwinCAT;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.Ads.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace TestApp
{
    internal class Program
    {
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct EthercatDevice
        {
            public bool mDirection;
            public bool t1;
            public bool t2;
            public bool t3;
            public UInt32 mSpeed;
            public UInt32 mTime;
            public bool mMode;
            public bool t4;
            public bool t5;
            public bool t6;
        }
        static void Main(string[] args)
        {
            // To Test the Observer run a project on the local PLC System (Port 851)
            using (AdsClient client = new AdsClient())
            {
                // Connect to target
                client.Connect(new AmsAddress(AmsNetId.Local, 851));
                IDynamicSymbolLoader loader = (IDynamicSymbolLoader)SymbolLoaderFactory.Create(client, SymbolLoaderSettings.DefaultDynamic);

                dynamic symbols = loader.SymbolsDynamic;
                dynamic main = symbols.Main;
                DynamicArrayInstance myStructSymbol = main.device; // Dynamically created
               var pp = myStructSymbol.ReadRawValue();
                Console.WriteLine();
            }
        }
    }
}
