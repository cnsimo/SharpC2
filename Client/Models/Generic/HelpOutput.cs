using System.Collections.Generic;

namespace SharpC2.Models.Generic
{
    public class HelpOutput : SharpSploitResult
    {
        public string Name { get; set; }
        public string Description { get; set; }

        protected internal override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() { Name = "Name", Value = Name },
                new() { Name = "Description", Value = Description }
            };
    }
}