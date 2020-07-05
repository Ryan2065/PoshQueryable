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
        $Script:p = $obj
    }
}

Write-Host "Initializing Collection"
[System.Collections.Generic.KeyValuePair[string, int][]]$InputObjects = (1..500000).ForEach({ [System.Collections.Generic.KeyValuePair[string, int]]::new("$_", $_) })

Write-Host "Invoking: Where-Object"
    $Measure = Measure-Command {
        $InputObjects | Where-Object { $_.Key -eq "5000" }
    }
    Write-Host "ExecutionTime: $($Measure.TotalSeconds)`r`n"

Write-Host "Invoking: .Where()"
    $Measure = Measure-Command {
        $InputObjects.Where({ $_.Key -eq "5000" })
    }
    Write-Host "ExecutionTime: $($Measure.TotalSeconds)`r`n"

Write-Host "Invoking: [System.Linq.Enumerable]::Where()"
    $Measure = Measure-Command {
        [System.Linq.Enumerable]::Where($InputObjects, [Func[[System.Collections.Generic.KeyValuePair[string, int]],bool]]{ param($x) $x.Key -eq "5000" })
    }
    Write-Host "ExecutionTime: $($Measure.TotalSeconds)`r`n"

Write-Host "Invoking: cmdlet Search-Queryable"
    $Measure = Measure-Command {
        $y = '59'
        $result = Search-Queryable -inputArray $InputObjects -Expression { $p.Key -eq $y }
        $result
    }
    Write-Host "ExecutionTime: $($Measure.TotalSeconds)`r`n"


