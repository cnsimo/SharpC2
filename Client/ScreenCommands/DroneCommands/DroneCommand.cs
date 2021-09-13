using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class DroneCommand : ScreenCommand
    {
        public override string Name { get; }
        public override string Description { get; }
        public override string Usage { get; }
        public override Screen.CommandCallback Callback { get; }

        public DroneCommand(string name, string description, string usage, Screen.CommandCallback callback)
        {
            Name = name;
            Description = description;
            Usage = usage;
            Callback = callback;
        }
    }
}