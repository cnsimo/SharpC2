using System.Collections.Generic;
using System.Text;

namespace SharpC2.Models
{
    public class HandlerParameter : SharpSploitResult
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Optional { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Current Value: {Value}");
            sb.AppendLine($"Optional: {Optional}");
            return sb.ToString();
        }

        protected internal override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() {Name = "Name", Value = Name},
                new() {Name = "Value", Value = Value},
                new() {Name = "Optional", Value = Optional}
            };
    }
}