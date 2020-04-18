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
RUN dotnet test "SmbAbstraction.Tests.Integration/SmbAbstraction.Tests.Integration.csproj" --logger:"console;verbosity=detailed"