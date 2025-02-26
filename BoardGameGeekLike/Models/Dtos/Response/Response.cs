using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class Response<T>
    {
        public T? Content { get; set; }

        public string? Message { get; set; }
    }
}