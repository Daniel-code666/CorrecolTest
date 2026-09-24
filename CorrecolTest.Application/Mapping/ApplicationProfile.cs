using AutoMapper;
using CorrecolTest.Application.Catalogos;
using CorrecolTest.Application.Clientes;
using CorrecolTest.Domain.Entities;

namespace CorrecolTest.Application.Mapping;

public class ApplicationProfile : Profile
{
    public ApplicationProfile()
    {
        CreateMap<ClienteCreateDto, Cliente>(MemberList.Source);
        CreateMap<ClienteUpdateDto, Cliente>(MemberList.Source);
        CreateMap<Cliente, ClienteDto>();
        CreateMap<Pais, PaisDto>();
        CreateMap<PaisCreateDto, Pais>(MemberList.Source);
        CreateMap<PaisUpdateDto, Pais>(MemberList.Source);
        CreateMap<DepartamentoCreateDto, Departamento>(MemberList.Source);
        CreateMap<DepartamentoUpdateDto, Departamento>(MemberList.Source);
        CreateMap<CiudadCreateDto, Ciudad>(MemberList.Source);
        CreateMap<CiudadUpdateDto, Ciudad>(MemberList.Source);
        CreateMap<Departamento, DepartamentoDto>();
        CreateMap<Ciudad, CiudadDto>()
            .ForMember(d => d.PaisCodigo, o => o.MapFrom(s => s.Departamento.PaisCodigo));
    }
}
