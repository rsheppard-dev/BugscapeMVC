FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Install Node.js
RUN apt-get update && \
    apt-get install -y wget xz-utils && \
    wget https://nodejs.org/dist/v14.18.1/node-v14.18.1-linux-x64.tar.xz && \
    tar -xf node-v14.18.1-linux-x64.tar.xz && \
    ln -s /node-v14.18.1-linux-x64/bin/node /usr/local/bin/

COPY ["BugscapeMVC.csproj", "./"]
RUN dotnet restore "BugscapeMVC.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "BugscapeMVC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BugscapeMVC.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BugscapeMVC.dll"]