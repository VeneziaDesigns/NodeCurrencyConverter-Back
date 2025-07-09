using AutoMapper;
using NodeCurrencyConverter.DTOs;
using NodeCurrencyConverter.Entities;
using NodeCurrencyConverter.Infrastructure.Models;

namespace NodeCurrencyConverter.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // DTO → Entity
            CreateMap<CurrencyDto, CurrencyEntity>()
                .ConstructUsing(dto => new CurrencyEntity(dto.Code));

            CreateMap<CurrencyExchangeDto, CurrencyExchangeEntity>()
                .ConstructUsing(dto => new CurrencyExchangeEntity(
                    new CurrencyEntity(dto.From),
                    new CurrencyEntity(dto.To),
                    dto.Value
                ));

            // Entity → DTO
            CreateMap<CurrencyEntity, CurrencyDto>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code));

            CreateMap<CurrencyExchangeEntity, CurrencyExchangeDto>()
                .ForCtorParam("From", opt => opt.MapFrom(src => src.From.Code))
                .ForCtorParam("To", opt => opt.MapFrom(src => src.To.Code))
                .ForCtorParam("Value", opt => opt.MapFrom(src => src.Value));

            // Entity ↔ Model
            CreateMap<CurrencyExchangeEntity, CurrencyExchangeModel>()
                .ForMember(dest => dest.From, opt => opt.MapFrom(src => src.From.Code))
                .ForMember(dest => dest.To, opt => opt.MapFrom(src => src.To.Code))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

            // Model ↔ Entity
            CreateMap<CurrencyExchangeModel, CurrencyExchangeEntity>()
                .ConstructUsing(model => new CurrencyExchangeEntity(
                    new CurrencyEntity(model.From),
                    new CurrencyEntity(model.To),
                    model.Value
                ));
        }
    }
}
