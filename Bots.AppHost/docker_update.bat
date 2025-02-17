REM Switch to the remote Docker context
docker context use remote

REM Build and tag the new image without using the cache
cd ../TenBot && dotnet publish --os linux --arch x64 /t:PublishContainer

REM Push the new image to the registry
docker push grandiras/tenbot:latest

REM --------------------------------------------

REM Stop and remove containers, then pull and start using docker-compose
cd ../Bots.AppHost/aspirate-output
docker-compose down
docker-compose pull
docker-compose up -d

REM Switch back to the default Docker context
docker context use default