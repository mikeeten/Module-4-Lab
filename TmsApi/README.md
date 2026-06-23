Before you begin: the TmsApi project:

Open your terminal and run these commands one time when you begin M4:
# Check your SDK version: must show 10.x
dotnet --version
# Create the Web API project (controller-based template; matches later exercises)
dotnet new webapi -n TmsApi --no-openapi --use-controllers
# Move into the project directory
cd TmsApi
# Open it in VS Code (or Visual Studio)
code .
Find Program.cs. You will rewrite it throughout this session. Confirm the project builds and runs:

dotnet run
You should see output like:
info: Microsoft.Hosting.Lifetime[14]
Now listening on: http://localhost:5xxx