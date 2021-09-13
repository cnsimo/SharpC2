using AutoMapper;

using SharpC2.API.V1.Responses;
using SharpC2.Models;

namespace SharpC2.Mapper
{
    public class DroneProfiles : Profile
    {
        public DroneProfiles()
        {
            CreateMap<DroneResponse, Drone>()
                .ForPath(d => d.Metadata.Guid, opt => opt.MapFrom(d => d.Guid))
                .ForPath(d => d.Metadata.Address, opt => opt.MapFrom(d => d.Address))
                .ForPath(d => d.Metadata.Hostname, opt => opt.MapFrom(d => d.Hostname))
                .ForPath(d => d.Metadata.Username, opt => opt.MapFrom(d => d.Username))
                .ForPath(d => d.Metadata.Process, opt => opt.MapFrom(d => d.Process))
                .ForPath(d => d.Metadata.Pid, opt => opt.MapFrom(d => d.Pid))
                .ForPath(d => d.Metadata.Integrity, opt => opt.MapFrom(d => d.Integrity))
                .ForPath(d => d.Metadata.Arch, opt => opt.MapFrom(d => d.Arch));

            CreateMap<DroneTaskResponse, DroneTask>();
            
            CreateMap<DroneModuleResponse, DroneModule>();
            CreateMap<DroneModuleResponse.CommandResponse, DroneModule.Command>();
            CreateMap<DroneModuleResponse.CommandResponse.ArgumentResponse, DroneModule.Command.Argument>();
        }
    }
}