color B

del  .PublishFiles\*.*   /s /q

dotnet restore

dotnet build

cd Student.Achieve

dotnet publish -o ..\Student.Achieve\bin\Debug\netcoreapp2.2\

md ..\.PublishFiles

xcopy ..\Student.Achieve\bin\Debug\netcoreapp2.2\*.* ..\.PublishFiles\ /s /e 

echo "Successfully!!!! ^ please see the file .PublishFiles"

cmd