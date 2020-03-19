#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG TAG=3.0

FROM mcr.microsoft.com/dotnet/core/runtime:${TAG}-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:${TAG}-buster AS build
WORKDIR /src
COPY ["SmbAbstraction.Tests.Integration/SmbAbstraction.Tests.Integration.csproj", "SmbAbstraction.Tests.Integration/"]
COPY ["SmbAbstraction/SmbAbstraction.csproj", "SmbAbstraction/"]
RUN dotnet restore "SmbAbstraction.Tests.Integration/SmbAbstraction.Tests.Integration.csproj"
RUN dotnet build "SmbAbstraction.Tests.Integration/SmbAbstraction.Tests.Integration.csproj" -c Debug
COPY . .

FROM build AS testrunner
RUN dotnet test "SmbAbstraction.Tests.Integration/SmbAbstraction.Tests.Integration.csproj" --logger:trx

#FROM build AS test
#RUN ls -la
#RUN dotnet test
#
#FROM build AS publish
#RUN dotnet publish "SmbAbstraction.Tests.Integration.csproj" -c Release -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "SmbAbstraction.Tests.Integration.dll"]