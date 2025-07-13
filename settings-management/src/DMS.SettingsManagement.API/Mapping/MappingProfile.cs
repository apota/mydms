using AutoMapper;
using DMS.SettingsManagement.Core.DTOs;
using DMS.SettingsManagement.Core.Entities;

namespace DMS.SettingsManagement.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Setting, SettingDto>();
            CreateMap<CreateSettingDto, Setting>();
            CreateMap<UpdateSettingDto, Setting>();
        }
    }
}
