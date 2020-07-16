# CSX-PowerShell-VisualStudio

C# Scripts (CSX) executed through Powershell directly from Visual Studio, and generating sample POCOs.

# Description

This project contains CSX Scripts (and associated C# classes) for generating POCOs based on a SQL Server schema of AdventureWorks database.

The generator uses [CodegenCS library](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS), 
and there is an associated PowerShell script which can be used to invoke the CSX (can be executed from Visual Studio or outside).

There are 2 projects (with slightly different CSX and PowerShell scripts) showing the solution both for SDK-Style Projects and for non-SDK-Style Projects:

Project | Type | Description
------------ | ------------- |-------------
CSX-PowerShell-VisualStudio2017 | SDK-Style Project | This is the latest csproj/vbproj (msbuild) format and it's the one used by netcore or projects created with Visual Studio 2017+. <br /> In this format the NuGet packages are stored per-user profile (NuGet 4).
CSX-PowerShell-VisualStudio2015 | Non-SDK-Style Project | This is the older csproj/vbproj (msbuild) format and it's the one used by projects created with Visual Studio <=2015. <br /> In this format the NuGet packages (NuGet <=3) are stored under solution folder. <br />In this csproj format the source files must be explicitly described in csproj. 


# Collaborate

This is just a sample project, mostly targeted as companion code for this Article on CodeProject, which is mostly about running CSX scripts.

If you want to collaborate on the POCOs generator, please refer to [**CodegenCS.POCO project**](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.POCO) where I'll maintain latest working templates.

## License
MIT License
