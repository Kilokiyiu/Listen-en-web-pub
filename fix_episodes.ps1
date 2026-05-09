$ErrorActionPreference = "Continue"
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=localhost;Database=Listen_En_Web_test_Listen.;Trusted_Connection=True;TrustServerCertificate=True"
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "UPDATE T_Episode SET IsVisible = 1 WHERE IsVisible = 0; SELECT @@ROWCOUNT as RowsAffected"
    $rows = $cmd.ExecuteScalar()
    Write-Host "Updated $rows episodes to IsVisible = 1"
    $conn.Close()
} catch {
    Write-Host "ERROR: $_"
}
