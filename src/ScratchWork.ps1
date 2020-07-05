param ( 
    $CommandName,
    $ParameterName,
    $WordToComplete,
    $CommandAst,
    $FakeBoundParameters
)
if($FakeBoundParameters.ContainsKey('InputArray')) {
    $InputArray = $FakeBoundParameters['InputArray']
    $obj = [PoshQueryable.PoshArgumentCompleter]::GetGenericObject($InputArray)
    if($null -ne $Script:qu){
        if($Script:qu.GetType().Name -eq $obj.GetType().Name){
            return
        }
    }
    $Script:qu = $obj
}