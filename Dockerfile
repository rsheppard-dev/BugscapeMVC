FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Install Node.js
RUN apt-get update && \
    apt-get install -y wget xz-utils && \
    wget https://nodejs.org/dist/v14.18.1/node-v14.18.1-linux-x64.tar.xz && \
    cd / && \
    tar -xf /src/node-v14.18.1-linux-x64.tar.xz && \
    ln -s /node-v14.18.1-linux-x64/bin/node /usr/local/bin/ && \
    ln -s /node-v14.18.1-linux-x64/bin/npm /usr/local/bin/

WORKDIR /src

# Copy package.json and package-lock.json (or yarn.lock)
COPY package*.json ./

# Install Node.js dependencies
RUN npm install

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