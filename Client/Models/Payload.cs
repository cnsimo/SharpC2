using System.Collections.Generic;
using System.Text;

namespace SharpC2.Models
{
    public class Payload : SharpSploitResult
    {
        public string Handler { get; set; } = "";
        public PayloadFormat Format { get; set; } = PayloadFormat.Exe;

        public enum PayloadFormat : int
        {
            Exe = 0,
            Dll = 1,
            PowerShell = 2,
            Raw = 3,
            Svc = 4
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            sb.AppendLine($"Handler: {Handler}");
            sb.AppendLine($"Format: {Format}");

            return sb.ToString();
        }

        protected internal override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() { Name = "Handler", Value = Handler },
                new() { Name = "Format", Value = Format }
            };
    }
}