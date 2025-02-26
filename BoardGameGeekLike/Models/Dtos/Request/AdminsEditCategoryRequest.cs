using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsEditCategoryRequest
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }
    }
}