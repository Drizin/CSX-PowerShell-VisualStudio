VisualStudio2015 used "non-SDK-style" projects, which use packages.config to describe NuGet packages, and use NuGet <=3.

How to run:

1) Build the Project, to restore Nuget references
2) Execute GenerateSimplePOCOs.ps1
3) csproj will be modified (to include the new generated POCO files) and you'll get a warning. Just click reload.