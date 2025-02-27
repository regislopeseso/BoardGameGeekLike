using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGameGeekLike.Models;

namespace BoardGameGeekLike.Services
{
    public class UsersService
    {
        private readonly ApplicationDbContext _daoDbContext;

        public UsersService(ApplicationDbContext daoDbContext)
        {
            this._daoDbContext = daoDbContext;
        }

    }
}