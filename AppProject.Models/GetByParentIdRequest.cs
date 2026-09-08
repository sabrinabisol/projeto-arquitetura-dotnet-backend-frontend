using System;

namespace AppProject.Models;

public class GetByParentIdRequest<TIdType> : IRequest
{
    required public TIdType ParentId { get; set; }
}
