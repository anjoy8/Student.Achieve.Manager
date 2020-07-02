git pull;
rm -rf .PublishFiles;
dotnet build;
dotnet publish -o /home/Student.Achieve.Manager/Student.Achieve.Api/Student.Achieve/bin/Debug/netcoreapp3.1;
cp -r /home/Student.Achieve.Manager/Student.Achieve.Api/Student.Achieve/bin/Debug/netcoreapp3.1 .PublishFiles;
echo "Successfully!!!! ^ please see the file .PublishFiles";