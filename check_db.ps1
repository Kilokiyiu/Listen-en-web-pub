Add-Type -AssemblyName "System.Data"
$conn = New-Object System.Data.SqlClient.SqlConnection("Server=localhost;Database=Listen_En_Web_test_Listen.;Trusted_Connection=True;TrustServerCertificate=True")
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT Id, Name_Chinese, Name_English, IsVisible, CategoryId FROM T_Album"
$reader = $cmd.ExecuteReader()
Write-Host "=== T_Album ==="
while($reader.Read()) {
    Write-Host "Id: $($reader['Id']) | Name: $($reader['Name_Chinese']) | IsVisible: $($reader['IsVisible']) | CatId: $($reader['CategoryId'])"
}
$reader.Close()
Write-Host ""
$cmd2 = $conn.CreateCommand()
$cmd2.CommandText = "SELECT Id, Name_Chinese, Name_English, IsVisible, AlbumId FROM T_Episode"
$reader2 = $cmd2.ExecuteReader()
Write-Host "=== T_Episode ==="
while($reader2.Read()) {
    Write-Host "Id: $($reader2['Id']) | Name: $($reader2['Name_Chinese']) | IsVisible: $($reader2['IsVisible']) | AlbumId: $($reader2['AlbumId'])"
}
$reader2.Close()
$conn.Close()
