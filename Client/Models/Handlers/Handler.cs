using System.Collections.Generic;
using System.Text;

namespace SharpC2.Models
{
    public class Handler : SharpSploitResult
    {
        public string Name { get; set; }
        public IEnumerable<HandlerParameter> Parameters { get; set; }
        public bool Running { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Running: {Running}");

            foreach (var parameter in Parameters)
                sb.AppendLine($"{parameter.Name}: {parameter.Value}");

            return sb.ToString();
        }

        protected internal override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() {Name = "Name", Value = Name},
                new() {Name = "Running", Value = Running}
            };
    }
}