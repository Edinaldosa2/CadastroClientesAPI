FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
RUN adduser --disabled-password --gecos "" --uid 10001 appuser \
    && mkdir -p /app/data /tmp \
    && chown -R appuser:appuser /app /tmp
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["global.json", "."]
COPY ["CadastroClientes/CadastroClientes.csproj", "CadastroClientes/"]
COPY ["CadastroCliente.Aplicação/CadastroCliente.Aplicação.csproj", "CadastroCliente.Aplicação/"]
COPY ["CadastroCliente.Service/CadastroCliente.Service.csproj", "CadastroCliente.Service/"]
COPY ["CadastroCliente.Data/CadastroCliente.Data.csproj", "CadastroCliente.Data/"]
COPY ["CadastroCliente.Domain/CadastroCliente.Domain.csproj", "CadastroCliente.Domain/"]
COPY ["Cadastro.CrossCutting/CadastroCliente.CrossCutting.csproj", "Cadastro.CrossCutting/"]
RUN dotnet restore "CadastroClientes/CadastroClientes.csproj"
COPY . .
WORKDIR "/src/CadastroClientes"
RUN dotnet build "CadastroClientes.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "CadastroClientes.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish --chown=appuser:appuser /app/publish .
USER appuser
ENTRYPOINT ["dotnet", "CadastroClientes.dll"]
