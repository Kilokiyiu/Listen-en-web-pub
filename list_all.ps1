$OutputEncoding = [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = "Continue"
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=localhost;Database=Listen_En_Web_test_Listen.;Trusted_Connection=True;TrustServerCertificate=True"
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = @"
    SELECT e.Id, e.Name_Chinese as EpisodeName, a.Name_Chinese as AlbumName, c.Name_Chinese as CategoryName, e.IsVisible, e.AudioUrl
    FROM T_Episode e
    INNER JOIN T_Album a ON e.AlbumId = a.Id
    INNER JOIN T_Category c ON a.CategoryId = c.Id
    ORDER BY c.SequenceNumber, a.SequenceNumber
"@
    $r = $cmd.ExecuteReader()
    Write-Host "=== 当前所有题目 ===" -ForegroundColor Cyan
    $idx = 1
    while ($r.Read()) {
        $visible = if ($r['IsVisible']) { "[显示]" } else { "[隐藏]" }
        $color = if ($r['IsVisible']) { "Green" } else { "Red" }
        Write-Host "$idx. $($r['EpisodeName']) | $($r['AlbumName']) | $visible" -ForegroundColor $color
        Write-Host "   ID: $($r['Id'])" -ForegroundColor Gray
        $idx++
    }
    $r.Close()

    Write-Host ""
    $cmd2 = $conn.CreateCommand()
    $cmd2.CommandText = "SELECT Id, Name_Chinese, IsVisible FROM T_Album ORDER BY SequenceNumber"
    $r2 = $cmd2.ExecuteReader()
    Write-Host "=== 当前所有试卷 ===" -ForegroundColor Cyan
    while ($r2.Read()) {
        $visible = if ($r2['IsVisible']) { "[显示]" } else { "[隐藏]" }
        $color = if ($r2['IsVisible']) { "Green" } else { "Red" }
        Write-Host "  * $($r2['Name_Chinese']) | $visible" -ForegroundColor $color
    }
    $r2.Close()
    $conn.Close()
} catch {
    Write-Host "ERROR: $_" -ForegroundColor Red
}
