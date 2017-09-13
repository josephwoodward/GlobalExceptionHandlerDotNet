rm -rf ./package
cd ./src/GlobalExceptionHandler/
dotnet pack ./GlobalExceptionHandler.csproj -o ../../package/ -c release