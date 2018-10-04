rem dotnet new lib -o mylibrary
rem dotnet new web -o aspnetcore
rem dotnet add reference ../mylibrary/mylibrary.csproj
pushd aspnetcore
dotnet run
popd

rem on a separate window,
rem start http://localhost:5000
