FROM alpine/git as clone

WORKDIR /app

RUN git clone https://github.com/sorface/interview-platform.git

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

# Copy everything
COPY --from=clone /app/interview-platform/Backend .

RUN dotnet restore

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0

WORKDIR /app

COPY --from=build-env /app/out .
#
#COPY wait.sh /WorkDirectory/wait.sh
#RUN chmod +x /WorkDirectory/wait.sh

EXPOSE 5043

ENTRYPOINT ["dotnet", "Interview.Backend.dll"]