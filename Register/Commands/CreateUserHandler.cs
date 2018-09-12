using Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Register.Commands
{
    public class CreateUserHandler : RequestHandler<CreateUser>
    {
        protected override void Handle(CreateUser request)
        {
            throw new NotImplementedException();
        }
    }
}
