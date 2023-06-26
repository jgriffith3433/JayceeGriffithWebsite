using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using FluentValidation;
using MediatR;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class CreateUserCommand : IRequest<AuthTokenDTO>
    {
        public CreateUserCommand(CreateOrUpdateUserDTO model)
        {
            Model = model;
        }

        public CreateOrUpdateUserDTO Model { get; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, AuthTokenDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly ITokenService _token;

        public CreateUserCommandHandler(IUnitOfWork repository, ITokenService token)
        {
            _repository = repository;
            _token = token;
        }

        public async Task<AuthTokenDTO> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            var user = _repository.Users.CreateProxy();
            {
                user.EmailAddress = model.EmailAddress;
                user.Password = model.Password;
                user.Role = model.Role;
            };

            _repository.Users.Add(user);
            await _repository.CommitAsync();

            return _token.Generate(user);
        }
    }
}
