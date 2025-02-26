using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsAddCategoryRequest
    {
        public required string CategoryName { get; set; }
    }
}