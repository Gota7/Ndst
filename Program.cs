using System;
using System.Collections.Generic;
using System.Linq;

namespace Ndst {

    class Program {

        static void Main(string[] args) {
            // ROM n = new ROM("NSMB.nds");
            // n.Extract("ROM");
            ROM r = new ROM("ROM", "Patch");
            r.Save("Test.nds");
        }

    }

}
