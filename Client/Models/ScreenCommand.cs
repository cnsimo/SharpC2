using System.Collections.Generic;

using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public abstract class ScreenCommand : SharpSploitResult
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Usage { get; }
        public abstract Screen.CommandCallback Callback { get; }

        protected internal override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() { Name = "Name", Value = Name },
                new() { Name = "Description", Value = Description }
            };
    }
}