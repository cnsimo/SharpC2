using System;
using System.Collections.Generic;

namespace SharpC2.Models
{
    public class Drone : SharpSploitResult
    {
        public DroneMetadata Metadata { get; set; }
        public DateTime LastSeen { get; set; }

        public List<DroneModule> Modules { get; set; } = new();

        // for automapper
        public Drone() { }

        public Drone(DroneMetadata metadata)
        {
            Metadata = metadata;
        }

        public double LastSeenSeconds
        {
            get
            {
                var time = (DateTime.UtcNow - LastSeen).TotalSeconds;
                return Math.Round(time, 2);
            }
        }

        public void CheckIn()
        {
            LastSeen = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"{Metadata.Username}@{Metadata.Hostname} ({Metadata.Process}:{Metadata.Pid})";
        }

        protected internal override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() {Name = "Guid", Value = Metadata.Guid},
                new() {Name = "Address", Value = Metadata.Address},
                new() {Name = "Hostname", Value = Metadata.Hostname},
                new() {Name = "Username", Value = Metadata.Username},
                new() {Name = "Process", Value = Metadata.Process},
                new() {Name = "PID", Value = Metadata.Pid},
                new() {Name = "Integrity", Value = Metadata.Integrity},
                new() {Name = "Arch", Value = Metadata.Arch},
                new() {Name = "LastSeen", Value = $"{LastSeenSeconds}s"}
            };
    }
}