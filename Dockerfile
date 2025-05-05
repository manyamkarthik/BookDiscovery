FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY BookDiscovery.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish BookDiscovery.csproj -c Release -o /app/publish

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src .

# Install EF Core runtime tools
RUN apt-get update && apt-get install -y curl
RUN curl -L https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
RUN chmod +x dotnet-install.sh
RUN ./dotnet-install.sh --version 8.0.0 --install-dir /usr/share/dotnet --runtime aspnetcore
RUN dotnet tool install --global dotnet-ef --version 8.0.0
ENV PATH="$PATH:/root/.dotnet/tools"

# Set environment variables
ENV ASPNETCORE_URLS=http://+:${PORT:-3000}
ENV ASPNETCORE_ENVIRONMENT=Production

# Create a startup script
RUN echo '#!/bin/sh\n\
echo "Applying database migrations..."\n\
dotnet ef database update --project /src/BookDiscovery.csproj || echo "Migration failed, continuing..."\n\
echo "Starting application..."\n\
exec dotnet BookDiscovery.dll' > /app/start.sh

RUN chmod +x /app/start.sh

ENTRYPOINT ["/app/start.sh"]
