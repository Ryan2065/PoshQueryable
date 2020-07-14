Import-Module "$PSScriptRoot\Dependencies\PoshQueryable.dll"
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