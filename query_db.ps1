$ErrorActionPreference = "Continue"
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=localhost;Database=Listen_En_Web_test_Listen.;Trusted_Connection=True;TrustServerCertificate=True"
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT Id, Name_Chinese, IsVisible, CategoryId FROM T_Album"
    $r = $cmd.ExecuteReader()
    Write-Host "=== T_Album ==="
    while ($r.Read()) {
        Write-Host "Id: $($r['Id']) | Name: $($r['Name_Chinese']) | IsVisible: $($r['IsVisible']) | CatId: $($r['CategoryId'])"
    }
    $r.Close()
    $cmd2 = $conn.CreateCommand()
    $cmd2.CommandText = "SELECT TOP 20 Id, Name_Chinese, IsVisible, AlbumId FROM T_Episode"
    $r2 = $cmd2.ExecuteReader()
    Write-Host "=== T_Episode ==="
    while ($r2.Read()) {
        Write-Host "Id: $($r2['Id']) | Name: $($r2['Name_Chinese']) | IsVisible: $($r2['IsVisible']) | AlbumId: $($r2['AlbumId'])"
    }
    $r2.Close()
    $conn.Close()
    Write-Host "DONE"
} catch {
    Write-Host "ERROR: $_"
}
