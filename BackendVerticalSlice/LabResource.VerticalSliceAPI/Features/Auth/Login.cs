using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Common.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LabResource.VerticalApi.Features.Auth;

public static class Login
{
    public record Command(string Email, string Password) : IRequest<Result?>;

    public record Result(string Token, string Email, string FullName);

    public class Handler : IRequestHandler<Command, Result?>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly JwtSettings _jwtSettings;

        public Handler(ApplicationDbContext dbContext, IOptions<JwtSettings> jwtOptions)
        {
            _dbContext = dbContext;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<Result?> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return null;
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                _jwtSettings.Issuer,
                _jwtSettings.Audience,
                claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            return new Result(
                new JwtSecurityTokenHandler().WriteToken(token),
                user.Email,
                user.FullName
            );
        }
    }
}