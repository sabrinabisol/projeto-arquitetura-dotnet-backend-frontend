using System;

namespace AppProject.Exceptions;

public class ExceptionDetail
{
    public ExceptionCode Code { get; set; }

    public string? AdditionalInfo { get; set; }
}
