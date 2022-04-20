using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperImposX
{
    public static class Helpers
    {
        public struct Bounds<T>
        {
            public T Min { get; set; }

            public T Max { get; set; }
        }
    }
}
