@SET FrameworkDir=C:\Windows\Microsoft.NET\Framework\v4.0.30319
@SET FrameworkVersion=v4.0.30319
@SET FrameworkSDKDir=
@SET PATH=%FrameworkDir%;%FrameworkSDKDir%;%PATH%
@SET LANGDIR=EN

-- Build X64 Build
msbuild.exe RiPRipper.sln /p:Configuration=RiPRipper64 /t:Clean;Build /p:WarningLevel=0 /p:Platform="x64"

-- Build X Build
msbuild.exe RiPRipper.sln /p:Configuration=RiPRipperX /t:Clean;Build /p:WarningLevel=0

-- Build Regular Build + Package 
msbuild.exe RiPRipper.sln /p:Configuration=RiPRipper /t:Clean;Build /p:WarningLevel=0