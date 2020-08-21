using System.Collections.Generic;

namespace HeboTech.ATLib.Parsers
{
    public class AtResponse
    {
        public bool Success { get; set; }
        public string FinalResponse { get; set; }
        public List<string> Intermediates { get; set; } = new List<string>();
    }
}
