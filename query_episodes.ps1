$ErrorActionPreference = "Continue"
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=localhost;Database=Listen_En_Web_test_Listen.;Trusted_Connection=True;TrustServerCertificate=True"
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = @"
    SELECT e.Id, e.Name_Chinese, a.Name_Chinese as AlbumName, e.IsVisible, e.AudioUrl
    FROM T_Episode e
    INNER JOIN T_Album a ON e.AlbumId = a.Id
    ORDER BY e.CreationTime DESC
"@
    $r = $cmd.ExecuteReader()
    Write-Host "=== Episodes ==="
    while ($r.Read()) {
        $visible = if ($r['IsVisible']) { "显示" } else { "隐藏" }
        Write-Host "Id: $($r['Id']) | $($r['Name_Chinese']) | 试卷:$($r['AlbumName']) | 状态:$visible | 音频:$($r['AudioUrl'])"
    }
    $r.Close()
    $conn.Close()
} catch {
    Write-Host "ERROR: $_"
}
