cd "$PSScriptRoot\PoshQueryable\PoshQueryable"

dotnet publish --self-contained --configuration release

cd "$PSScriptRoot"


if(Test-Path "$PSScriptRoot\Module\PoshQueryable\Dependencies"){
    & cmd /c rd "$PSScriptRoot\Module\PoshQueryable\Dependencies\" /s /q
}

$Files = "$PSScriptRoot\PoshQueryable\PoshQueryable\bin\Release\netstandard2.0\publish\*"

$null = New-Item "$PSScriptRoot\Module\PoshQueryable\Dependencies" -ItemType Directory -Force

Copy-Item -Path $Files -Destination "$PSScriptRoot\Module\PoshQueryable\Dependencies\" -Force -Recurse
