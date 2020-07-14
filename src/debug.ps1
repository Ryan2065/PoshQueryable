Import-Module 'C:\Users\Ryan2\source\repos\PoshQueryable\src\PoshQueryable\PoshQueryable\bin\Debug\netstandard2.0\PoshQueryable.dll'

Register-ArgumentCompleter -CommandName Search-Queryable -ParameterName Expression -ScriptBlock {
    param ( 
        $CommandName,
        $ParameterName,
        $WordToComplete,
        $CommandAst,
        $FakeBoundParameters
    )
    if($FakeBoundParameters.ContainsKey('InputArray')) {
        '{'
        $InputArray = $FakeBoundParameters['InputArray']
        $obj = [PoshQueryable.PoshArgumentCompleter]::GetGenericObject($InputArray)
        if($null -ne $Script:p){
            if($Script:p.GetType().Name -eq $obj.GetType().Name){
                return
            }
        }
        $Script:_ = $obj
    }
}

Write-Host "Initializing Collection"

Class TypeOne{
    [string]$Key
}
Class TypeTwo{
    [TypeOne]$One
    [int]$Count
}

$List = New-Object System.Collections.Generic.List[TypeTwo]
0..100 | ForEach-Object {
    $one = [TypeOne]::new()
    $one.Key = "a$_"
    $two = [TypeTwo]::new()
    $two.One = $one
    $two.Count = $_
    $List.Add($two)
}
$one = [TypeOne]::new()
$one.Key = "a2"
$two = [TypeTwo]::new()
$two.One = $one
$two.Count = 4
$List.Add($two)

Search-Queryable -inputArray $List -Expression { $_.One.Key -eq 'a1' -or ( ($_.One.Key -eq 'a2') -and ($_.Count -eq 2) ) }

return
