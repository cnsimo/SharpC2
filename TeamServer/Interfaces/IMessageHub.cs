using System.Threading.Tasks;

using SharpC2.API.V1.Responses;

using TeamServer.Models;

namespace TeamServer.Interfaces
{
    public interface IMessageHub
    {
        Task HandlerLoaded(HandlerResponse handler);
        Task HandlerStarted(HandlerResponse handler);
        Task HandlerStopped(HandlerResponse handler);

        Task HostedFileAdded(string filename);
        Task HostedFileDeleted(string filename);

        Task DroneCheckedIn(DroneMetadata metadata);
        Task DroneModuleLoaded(DroneMetadata metadata, DroneModule module);
        Task DroneTasked(DroneMetadata metadata, DroneTaskResponse task);
        Task DroneDataSent(DroneMetadata metadata, int messageSize);
        Task DroneTaskRunning(DroneMetadata metadata, DroneTaskUpdate task);
        Task DroneTaskComplete(DroneMetadata metadata, DroneTaskUpdate task);
        Task DroneTaskCancelled(DroneMetadata metadata, DroneTaskUpdate task);
        Task DroneTaskAborted(DroneMetadata metadata, DroneTaskUpdate task);
    }
}