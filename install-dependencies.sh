sudo mozroots --import --sync
curl -L http://nuget.org/nuget.exe -o nuget.exe

mono nuget.exe install FAKE -OutputDirectory packages -ExcludeVersion -Prerelease
