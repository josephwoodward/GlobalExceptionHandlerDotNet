rm -rf ./package

cd ./src/

dotnet pack ./GlobalExceptionHandler/GlobalExceptionHandler.csproj -o ../../package/ -c release
dotnet pack ./GlobalExceptionHandler.ContentNegotiation.Mvc/GlobalExceptionHandler.ContentNegotiation.Mvc.csproj -o ../../package/ -c release