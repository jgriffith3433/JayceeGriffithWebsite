using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using FluentValidation;
using MediatR;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class ValidateUserCommand : IRequest<AuthTokenDTO>
    {
        public ValidateUserCommand(ValidateUserDTO model)
        {
            Model = model;
        }

        public ValidateUserDTO Model { get; }
    }

    public class ValidateUserCommandHandler : IRequestHandler<ValidateUserCommand, AuthTokenDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly ITokenService _token;

        public ValidateUserCommandHandler(IUnitOfWork repository, ITokenService token)
        {
            _repository = repository;
            _token = token;
        }

        public async Task<AuthTokenDTO> Handle(ValidateUserCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            var entities = _repository.Users.GetAll().Where(x => x.EmailAddress == model.EmailAddress);
            if (!entities.Any()) throw new UnauthorizedAccessException($"No Users matching emailAddress {model.EmailAddress} found");

            var user = entities.Where(x => x.Password == model.Password).FirstOrDefault();
            if(user == null) throw new UnauthorizedAccessException($"Passwords donot match. Authentication Failed.");

            return await Task.FromResult(_token.Generate(user));
        }
    }
}
