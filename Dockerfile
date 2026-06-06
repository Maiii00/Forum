FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Forum/Forum.csproj", "Forum/"]
RUN dotnet restore "Forum/Forum.csproj"

COPY . .
WORKDIR "/src/Forum"
RUN dotnet build "Forum.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Forum.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:7254
EXPOSE 7254

ENTRYPOINT ["dotnet", "Forum.dll"]