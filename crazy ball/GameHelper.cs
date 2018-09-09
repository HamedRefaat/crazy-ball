using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;

namespace WPFGame1
{
    public partial class Gammer
    {
        public enum currentToggle
        {
            Red,
            Blue
        };

        public class coordinates
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}
