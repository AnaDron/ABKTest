using System.Collections.Generic;

namespace ABKTest.Options
{
    public class ApplicationOptions
    {
        public int Port { get; set; } = 5000;
        public ICollection<string> Servers { get; set; } = new List<string>();
    }
}
