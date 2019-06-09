using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetAttacher.Window.Model
{
    public class DotnetProcess
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string ShortPointName { get; set; }
        public string LongPointName { get; set; }
        public bool IsSelected { get; set; }
        public bool Attached { get; set; }
    }
}
