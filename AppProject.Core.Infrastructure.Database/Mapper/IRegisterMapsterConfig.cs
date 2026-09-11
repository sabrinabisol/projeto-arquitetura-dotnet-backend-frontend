using System;
using Mapster;

namespace AppProject.Core.Infrastructure.Database.Mapper;

public interface IRegisterMapsterConfig
{
    // Interface de configuração de Entidade x DTO
    void Register(TypeAdapterConfig config);
}
