using System;
using System.Collections.Generic;
using System.Text;

namespace Setre.Common
{
    public interface IResult
    {
        bool IsSuccess { get; set; }
        string Message { get; set; }
    }
}
