FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /app
# this is a new separate directory inside docker nto our filesystem
EXPOSE 80

# copy all .csproj files and restore as distinct layers.   Use of the same COPY command
# for every dockerfile in the project to take advantage of docker caching
COPY Carsties.sln Carsties.sln
COPY src/AuctionService/AuctionService.csproj src/AuctionService/AuctionService.csproj
COPY src/SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj

# Restore package deps
RUN dotnet restore Carsties.sln
# takes a look at Carsties.sln and all the .csproj files linked to it

# Copy the app folders over
COPY src/AuctionService src/AuctionService
COPY src/Contracts src/Contracts
# copy Contracts because AuctionService depends on it
WORKDIR /app/src/AuctionService
# use the new separate directory inside docker
# /app + /src/AuctionService = /app/src/AuctionService
# /app is coming fron line 2
# /src/AuctionService is coming from line 9 where our AuctionService.csproj is located
RUN dotnet publish -c Release -o /app/src/out
# -c Release is the configuration and -o is the output directory

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
# much smaller dotnet run time from dotnet/sdk in line 1
WORKDIR /app
# copy everything from the base "build" image created in line 1 to the new separate directory inside docker at /app/src/out/****
# use the output from dotnet publish in the previous stage
COPY --from=build /app/src/out .
# set up the entry point for the application
ENTRYPOINT [ "dotnet", "AuctionService.dll" ]