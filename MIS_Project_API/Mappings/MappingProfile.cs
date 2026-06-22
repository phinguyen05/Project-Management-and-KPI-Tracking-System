using System;
using AutoMapper;
using MIS_Project_API.Models;
using MIS_Project_API.DTOs.Project;

namespace MIS_Project_API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map từ Model (DB) ra DTO (Trả về Frontend)
            // Lấy FullName của User gán vào ManagerName của DTO
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.Manager != null ? src.Manager.FullName : "N/A"))
                .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.ManagerId ?? 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.HasValue ? src.StartDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Deadline.HasValue ? src.Deadline.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null));

            // Map từ DTO (Frontend gửi lên) vào Model (DB)
            CreateMap<CreateProjectDto, Project>();
            CreateMap<UpdateProjectDto, Project>();

            // --- MAPPING CHO TASK ---
            // Map từ Model ra DTO (Lấy tên người được giao việc gán vào AssigneeName)
            CreateMap<MIS_Project_API.Models.Task, MIS_Project_API.DTOs.Task.TaskDto>()
                .ForMember(dest => dest.AssigneeName, opt => opt.MapFrom(src => src.Assignee != null ? src.Assignee.FullName : "N/A"));

            // Map từ CreateDto vào Model
            CreateMap<MIS_Project_API.DTOs.Task.CreateTaskDto, MIS_Project_API.Models.Task>();
        }
    }
}