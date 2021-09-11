using System.Threading.Tasks;

using SharpC2.API.V1.Responses;

using TeamServer.Models;

namespace TeamServer.Interfaces
{
    public interface IMessageHub
    {
        Task HandlerLoaded(string handler);
        Task HandlerStarted(HandlerResponse handler);
        Task HandlerStopped(HandlerResponse handler);

        Task HostedFileAdded(string filename);
        Task HostedFileDeleted(string filename);

        Task DroneCheckedIn(string droneGuid);
        Task DroneModuleLoaded(string droneGuid, DroneModule module);
        Task DroneDataSent(string droneGuid, int messageSize);
        Task DroneTasked(string droneGuid, string taskGuid);
        Task DroneTaskRunning(string droneGuid, byte[] result);
        Task DroneTaskComplete(string droneGuid, byte[] result);
        Task DroneTaskCancelled(string droneGuid, string taskGuid);
        Task DroneTaskAborted(string droneGuid, byte[] error);
    }
}