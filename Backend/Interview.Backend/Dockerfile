FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ARG Database
ARG ConnectionString
ARG ClientId
ARG ClientSecret
ARG SwaggerPrefix
ARG RedisConnectionString
ARG UseRedis

ENV INTERVIEW_BACKEND_ConnectionStrings__type ${Database}
ENV INTERVIEW_BACKEND_ConnectionStrings__database ${ConnectionString}
ENV INTERVIEW_BACKEND_OAuthServiceDispatcher__0__ClientId ${ClientId}
ENV INTERVIEW_BACKEND_OAuthServiceDispatcher__0__ClientSecret ${ClientSecret}
ENV INTERVIEW_BACKEND_SwaggerOption__RoutePrefix ${SwaggerPrefix}
ENV INTERVIEW_BACKEND_EventStorage__RedisConnectionString ${RedisConnectionString}
ENV INTERVIEW_BACKEND_EventStorage__UseRedis ${UseRedis}

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Interview.Backend/Interview.Backend.csproj", "Interview.Backend/"]
COPY ["Interview.DependencyInjection/Interview.DependencyInjection.csproj", "Interview.DependencyInjection/"]
COPY ["Interview.Domain/Interview.Domain.csproj", "Interview.Domain/"]
COPY ["Interview.Infrastructure/Interview.Infrastructure.csproj", "Interview.Infrastructure/"]
COPY ["Interview.Migrations.Postgres/Interview.Migrations.Postgres.csproj", "Interview.Migrations.Postgres/"]
COPY ["Interview.Migrations.Sqlite/Interview.Migrations.Sqlite.csproj", "Interview.Migrations.Sqlite/"]
RUN dotnet restore "Interview.Backend/Interview.Backend.csproj"
COPY . .
WORKDIR "/src/Interview.Backend"
RUN dotnet build "Interview.Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Interview.Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Interview.Backend.dll"]
