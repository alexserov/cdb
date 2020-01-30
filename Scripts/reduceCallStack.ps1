#[0-9abcdef]{8}\s[0-9abcdef]{8}\s.*
$regex = "[0-9abcdef]{8}\s[0-9abcdef]{8,16}\s(?<methodname>.*)";
$prev = "";
$prev2 = "";

$result = "";
$count = 0;
$path = $args[0];
Get-Content $path | ForEach-Object {
    $line = $_;
    $line2 = $line    
    if($line -match $regex){
        $line2 = "XXXXXXXX XXXXXXXX $($Matches["methodname"])";        
    }
    $sameline = $line2 -eq $prev2;

    if($sameline -eq $true){
        $line = $line2;
        $count++;
    } else {
        $result+="`n";
        $result+=$prev;
        if($count -ne 0){
            $result+="`n========== RECURSIVE CALL {$count} TIMES ==========`n";
            $count = 0;
            $result+=$prev;
        }
    }
    $prev = $line;      
    $prev2 = $line2;
}
Remove-Item $args[0]
$result | Out-File $args[0]