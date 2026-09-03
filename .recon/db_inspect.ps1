# 只读数据库核查脚本（recon）v3 — 使用 cmd /c 避免 PS 原生命令 stderr 包装
# 用途：SHOW TABLES + 逐表 SHOW COLUMNS，供 STATE.md / CONTRACTS.md 使用
# 仅执行只读查询，不修改任何数据。
$ErrorActionPreference = 'Stop'
$mysql = "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"
$hp = "localhost"; $prt = "3306"; $usr = "root"; $pwd = "123456"; $db = "party_school"
$outDir = Split-Path -Parent $MyInvocation.MyCommand.Path

function Invoke-MySql([string]$sql) {
    $inner = "`"$mysql`" -h $hp -P $prt -u $usr -p$pwd -D $db --default-character-set=utf8mb4 --batch --skip-column-names -e `"$sql`" 2>nul"
    $raw = cmd /c $inner
    return @($raw)
}

$tables = @(Invoke-MySql "SHOW TABLES;")
$tables = @($tables | Where-Object { $_ -match '^[A-Za-z0-9_]+$' })
$tables = $tables | Sort-Object -Unique
$tableListFile = Join-Path $outDir "db_tables.txt"
[System.IO.File]::WriteAllLines($tableListFile, $tables, (New-Object System.Text.UTF8Encoding($false)))
Write-Output ("TABLE_COUNT=" + $tables.Count)
Write-Output "--- TABLES ---"
$tables | ForEach-Object { Write-Output $_ }

$colsOut = Join-Path $outDir "db_columns.txt"
$sb = New-Object System.Text.StringBuilder
foreach ($t in $tables) {
    [void]$sb.AppendLine("===== TABLE: $t =====")
    $cols = @(Invoke-MySql ("SHOW COLUMNS FROM ``" + $t + "``;"))
    foreach ($c in $cols) { [void]$sb.AppendLine($c) }
    [void]$sb.AppendLine("")
}
[System.IO.File]::WriteAllText($colsOut, $sb.ToString(), (New-Object System.Text.UTF8Encoding($false)))
Write-Output ("COLUMNS_WRITTEN=" + $colsOut)
