# To Execute Powershell Scripts from Visual Studio:
# 1) Right-button PS1 file - "Open With...""
# 2) Configure:
#      Program: Powershell.exe
#      Arguments: -noexit -File %1
#      Friendly Name: Execute PowerShell Script

# To execute CSX scripts you'll need CSI.EXE (C# REPL) which is shipped with Visual Studio
# but can also be installed by using the NuGet package Microsoft.Net.Compilers.Toolset - https://www.nuget.org/packages/Microsoft.Net.Compilers.Toolset/

# For more info about launching CSX scripts from PowerShell or from Visual Studio, check https://drizin.io/code-generation-csx-scripts-part1/

$dir = Split-Path $MyInvocation.MyCommand.Path 
$script = Join-Path $dir ".\GenerateSimplePOCOs.csx"


# Locate CSI.EXE by searching common paths
$csi = ( 
    "$Env:userprofile\.nuget\packages\microsoft.net.compilers.toolset\3.6.0\tasks\net472\csi.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\Roslyn\csi.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\Roslyn\csi.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\Roslyn\csi.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\Roslyn\csi.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\Roslyn\csi.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\Roslyn\csi.exe"
) | Where-Object { Test-Path $_ } | Select-Object -first 1

if (!$csi)
{
    Write-Host "---------------------------------------" -for red
    Write-Host "Can't find csi.exe" -for red
    Write-Host "Please fix search paths above, or install NuGet Microsoft.Net.Compilers.Toolset" -for red
    Write-Host "---------------------------------------" -for red
    Exit 1
}



$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

# Call csi.exe and specify that libraries referenced by #r directives should search in a few nuget locations

# New NuGet 4.0+ (PackageReference) saves User-specific packages in %userprofile%\.nuget\packages\
$nuget1 = "${env:userprofile}\.nuget\packages\";

# New NuGet 4.0+ (PackageReference) saves Machine-wide packages in %ProgramFiles(x86)%\Microsoft SDKs\NuGetPackages\"
$nuget2 = "${env:ProgramFiles(x86)}\Microsoft SDKs\NuGetPackages\";

# Old NuGet (packages.config) saves packages in "\packages" folder at solution level.
# Locate by searching a few levels above
$nuget3 = ( 
    (Join-Path $dir ".\packages\"),
    (Join-Path $dir "..\packages\"),
    (Join-Path $dir "..\..\packages\"),
    (Join-Path $dir "..\..\..\packages\"),
    (Join-Path $dir "..\..\..\..\packages\")
) | Where-Object { Test-Path $_ } | Select-Object -first 1


Write-host "Starting csi.exe $script ..." -for yellow
#& $csi /lib:"$nuget1" $script  # if you're using new NuGet format (PackageReference defined inside csproj)
& $csi /lib:"$nuget3" $script  # if you're using old NuGet format (packages.config)



Write-Host "Finished in $($stopwatch.Elapsed.TotalMilliSeconds) milliseconds"

# Since I configured "-noexit" parameter in Visual Studio I don't need this
#if ($host.Name -notmatch 'ISE') { Write-Host -NoNewLine "(Just press Enter to exit)" -for cyan; read-host; }  
