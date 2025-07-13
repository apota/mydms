@echo off
echo Running Sales Management Tests...

echo.
echo Running Backend Tests...
dotnet test tests\DMS.SalesManagement.Core.Tests\DMS.SalesManagement.Core.Tests.csproj

echo.
echo Running Frontend Tests...
cd tests\DMS.SalesManagement.Web.Tests
npm test

echo.
echo All tests completed!
