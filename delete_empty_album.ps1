$OutputEncoding = [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=localhost;Database=Listen_En_Web_test_Listen.;Trusted_Connection=True;TrustServerCertificate=True"
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "DELETE FROM T_Album WHERE Id = '312ae7c5-8b5e-4ad7-a275-3a769a2d7dfc'"
$rows = $cmd.ExecuteNonQuery()
Write-Host "已删除 $rows 条空试卷记录"

$conn.Close()
