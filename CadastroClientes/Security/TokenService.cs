using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CadastroCliente.Aplicacao.Abstractions;
using CadastroCliente.Aplicacao.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CadastroClientes.Security;

public sealed class TokenService
{
    private readonly JwtOptions _jwt;
    private readonly IReadOnlyList<AuthUser> _users;

    public TokenService(IOptions<JwtOptions> jwt, IConfiguration configuration)
    {
        _jwt = jwt.Value;
        _users = configuration.GetSection("Users").Get<List<AuthUser>>() ?? new List<AuthUser>();
    }

    public TokenResponse Emitir(LoginRequest request)
    {
        var user = _users.FirstOrDefault(u =>
            string.Equals(u.Usuario, request.Usuario, StringComparison.OrdinalIgnoreCase)
            && u.Senha == request.Senha);
        if (user is null)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        var roles = RolesDoPerfil(user.Perfil);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Usuario),
            new(ClaimTypes.Name, user.Usuario)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var token = new JwtSecurityToken(
            _jwt.Issuer,
            _jwt.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiresMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new TokenResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresIn = _jwt.ExpiresMinutes * 60,
            Perfil = user.Perfil
        };
    }

    public IReadOnlyList<UsuarioAdminDto> ListarUsuarios()
        => _users
            .Select(u => new UsuarioAdminDto { Usuario = u.Usuario, Perfil = u.Perfil })
            .ToList();

    public static string[] RolesDoPerfil(string perfil)
    {
        if (perfil.Equals(Perfis.Admin, StringComparison.OrdinalIgnoreCase))
        {
            return new[] { Perfis.Admin, Perfis.Escrita, Perfis.Leitura };
        }

        if (perfil.Equals(Perfis.Escrita, StringComparison.OrdinalIgnoreCase))
        {
            return new[] { Perfis.Leitura, Perfis.Escrita };
        }

        return new[] { Perfis.Leitura };
    }
}
