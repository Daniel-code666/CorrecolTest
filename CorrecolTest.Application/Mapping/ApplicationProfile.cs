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
        CreateMap<Departamento, DepartamentoDto>();
        CreateMap<Ciudad, CiudadDto>()
            .ForMember(d => d.PaisCodigo, o => o.MapFrom(s => s.Departamento.PaisCodigo));
    }
}
