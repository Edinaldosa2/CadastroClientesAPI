using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using CadastroCliente.Aplicacao.Abstractions;
using CadastroCliente.Data.Context;
using CadastroCliente.Data.Seed;
using CadastroCliente.Service.DI;
using CadastroClientes.Infrastructure;
using CadastroClientes.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    var jwtKey = builder.Configuration["Jwt:Key"] ?? string.Empty;
    if (jwtKey.Length < 32 || jwtKey.Contains("dev-only", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Jwt:Key de produção inválida. Defina uma chave com pelo menos 32 caracteres.");
    }
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<TokenService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<ICorrelationContext, HttpCorrelationContext>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"),
            new QueryStringApiVersionReader("api-version"));
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key))
{
    jwt.Key = "dev-only-key-change-in-production-32ch";
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cadastro de Clientes API",
        Version = "v1",
        Description = "API REST para cadastro de pessoas físicas e jurídicas, com endereços, contatos, status e relatórios.",
        Contact = new OpenApiContact
        {
            Name = "Edinaldo Sá",
            Url = new Uri("https://github.com/Edinaldosa2")
        },
        License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
    });

    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml))
    {
        options.IncludeXmlComments(xml, true);
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT no header Authorization: Bearer {token}. Perfis: leitura e escrita.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? "Data Source=cadastro-clientes.db";
if (EhSqliteMemoria(connectionString) && !builder.Environment.IsEnvironment("Testing"))
{
    throw new InvalidOperationException("SQLite InMemory é bloqueado no host. Use arquivo ou volume.");
}

builder.Services.AddDbContext<CadastroClienteContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddCadastroRepositories();
builder.Services.AddCadastroServices();
builder.Services.AddSingleton<IdempotencyStore>();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CadastroClienteContext>("sqlite")
    .AddCheck<SqliteFileHealthCheck>("sqlite-file");

var origins = builder.Configuration.GetSection("Security:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod();
        if (builder.Environment.IsProduction() && origins.Length > 0)
        {
            policy.WithOrigins(origins).AllowCredentials();
        }
        else
        {
            policy.SetIsOriginAllowed(_ => true);
        }
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("fixed", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseCors("Default");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SwaggerProtectionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Cadastro de Clientes v1");
    options.DocumentTitle = "Cadastro de Clientes API";
    options.DisplayRequestDuration();
});

app.MapControllers().RequireRateLimiting("fixed");
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true }).AllowAnonymous();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Name is "sqlite" or "sqlite-file"
}).AllowAnonymous();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription().AllowAnonymous();

if (!app.Environment.IsEnvironment("Testing") && !app.Environment.IsProduction())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<CadastroClienteContext>();
    await db.Database.EnsureCreatedAsync();
    if (app.Environment.IsDevelopment())
    {
        await ClienteSeed.EnsureSeedAsync(db);
    }
}

if (app.Environment.IsProduction())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<CadastroClienteContext>();
    await db.Database.EnsureCreatedAsync();
}

await app.RunAsync();

static bool EhSqliteMemoria(string connectionString)
    => connectionString.Contains("InMemory", StringComparison.OrdinalIgnoreCase)
       || connectionString.Contains("Mode=Memory", StringComparison.OrdinalIgnoreCase)
       || connectionString.Contains(":memory:", StringComparison.OrdinalIgnoreCase);

public partial class Program;
