using System;

namespace AppProject.Models;

public class GetByIdRequest<TIdType> : IRequest
{
    required public TIdType Id { get; set; }
}
