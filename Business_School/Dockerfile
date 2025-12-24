FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solution y proyecto
COPY ["Business_School.sln", "./"]
COPY ["Business_School/Business_School.csproj", "Business_School/"]

# Restaurar dependencias
RUN dotnet restore "Business_School/Business_School.csproj"

# Copiar todo el código
COPY . .

# Compilar
WORKDIR "/src/Business_School"
RUN dotnet build "Business_School.csproj" -c Release -o /app/build

# Publicar
FROM build AS publish
RUN dotnet publish "Business_School.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Business_School.dll"]