using System;

namespace AppProject.Models;

public class DeleteRequest<TIdType> : IRequest
{
    required public TIdType Id { get; set; }
}
