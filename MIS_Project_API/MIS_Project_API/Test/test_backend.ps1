# ============================================================
#  MIS Project API - Backend Smoke Test (Full Flow)
#  Chay tuan tu qua cac API chinh de kiem tra he thong con song
#  Cach dung: mo PowerShell, cd vao thu muc project,
#  chay `dotnet run` o mot cua so khac, roi dan file nay vao roi Enter.
# ============================================================

# ---- CAU HINH: SUA LAI CHO DUNG VOI MAY BAN ----
$baseUrl = "https://localhost:7010/api"   # <-- SUA PORT DUNG
$adminUsername = "admin"                  # <-- Username Admin 
$adminPassword = "123"                    # <-- Password tuong ung
# --------------------------------------------------

# Bo qua loi SSL certificate tu ky cho localhost
if ($PSVersionTable.PSVersion.Major -ge 7) {
    $PSDefaultParameterValues['Invoke-RestMethod:SkipCertificateCheck'] = $true
} else {
    if (-not ("TrustAllCertsPolicy" -as [type])) {
        Add-Type @"
using System.Net;
using System.Security.Cryptography.X509Certificates;
public class TrustAllCertsPolicy : ICertificatePolicy {
    public bool CheckValidationResult(ServicePoint sp, X509Certificate cert, WebRequest req, int problem) { return true; }
}
"@
    }
    [System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
}

$script:pass = 0
$script:fail = 0
$now = Get-Date # Lay thoi gian hien tai cho toan bo script

function Step($name, [scriptblock]$action) {
    Write-Host "`n--- $name ---" -ForegroundColor Cyan
    try {
        $result = & $action
        Write-Host "OK" -ForegroundColor Green
        $script:pass++
        return $result
    } catch {
        $msg = $_.ErrorDetails.Message
        if (-not $msg) { $msg = $_.Exception.Message }
        Write-Host "THAT BAI: $msg" -ForegroundColor Red
        $script:fail++
        return $null
    }
}

function Decode-Jwt($token) {
    $payload = $token.Split('.')[1]
    switch ($payload.Length % 4) { 2 { $payload += '==' } 3 { $payload += '=' } }
    $payload = $payload.Replace('-', '+').Replace('_', '/')
    $json = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payload))
    return $json | ConvertFrom-Json
}

Write-Host "============================================" -ForegroundColor Yellow
Write-Host " BAT DAU TEST - $baseUrl" -ForegroundColor Yellow
Write-Host " (Nho chay 'dotnet run' o cua so khac truoc!)" -ForegroundColor Yellow
Write-Host "============================================" -ForegroundColor Yellow

# 1. REGISTER
Write-Host "`n--- 1. Register Admin dau tien (neu DB con trong) ---" -ForegroundColor Cyan
try {
    $body = @{ Username = $adminUsername; Password = $adminPassword; FullName = "Admin Test" } | ConvertTo-Json
    Invoke-RestMethod -Uri "$baseUrl/auth/register" -Method Post -Body $body -ContentType "application/json" | Out-Null
    Write-Host "OK - Da tao Admin moi" -ForegroundColor Green
    $pass++
} catch {
    $errMsg = $_.ErrorDetails.Message
    if ($errMsg -and $errMsg -like "*đã có Admin*") {
        Write-Host "OK - He thong da co Admin tu truoc (dung thiet ke khoa Register)" -ForegroundColor Green
        $pass++
    } else {
        Write-Host "THAT BAI: $errMsg" -ForegroundColor Red
        $fail++
    }
}

# 2. LOGIN
$loginResult = Step "2. Login Admin" {
    $body = @{ Username = $adminUsername; Password = $adminPassword } | ConvertTo-Json
    Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $body -ContentType "application/json"
}

if (-not $loginResult) {
    Write-Host "`nKHONG LOGIN DUOC - dung lai script. Kiem tra lai Username/Password o dau file." -ForegroundColor Red
    Write-Host "Ket qua: $pass thanh cong / $fail that bai" -ForegroundColor Yellow
    return
}

$token = $loginResult.token
$headers = @{ Authorization = "Bearer $token" }
$claims = Decode-Jwt $token
$adminId = ($claims.PSObject.Properties | Where-Object { $_.Name -like "*nameidentifier*" -or $_.Name -eq "nameid" }).Value
Write-Host "Dang nhap OK. UserId = $adminId" -ForegroundColor Green

# 2.1. TAO TAI KHOAN NHAN VIEN (CRUD User API)
Step "2.1. Tao tai khoan nhan vien (CRUD User)" {
    $body = @{ 
        FullName = "Test Employee"
        Username = "emp_auto_$($now.Ticks)" # Chuyen ticks de khong bi trung lặp khi chay script nhieu lan
        Email = "emp_$($now.Ticks)@test.com"
        Password = "123"
        Role = "Employee" 
    } | ConvertTo-Json
    Invoke-RestMethod -Uri "$baseUrl/user" -Method Post -Body $body -ContentType "application/json" -Headers $headers
}

# 2.2. THIET LAP SYSTEM CONFIG
Step "2.2. Thiet lap System Config (Ky danh gia)" {
    $monthYear = $now.ToString("MM") + "/" + $now.Year
    $body = @{
        MonthYear = $monthYear
        StandardWorkingHours = 160
        Holidays = 0
        PenaltyFactor = "{`"1-2`":0.8,`"3-5`":0.5,`"over5`":0.0}"
    } | ConvertTo-Json
    Invoke-RestMethod -Uri "$baseUrl/systemconfig" -Method Post -Body $body -ContentType "application/json" -Headers $headers
}

# 3. TAO PROJECT
$project = Step "3. Tao Project moi" {
    $body = @{
        Name = "Du an Test Tu Dong"; Description = "Tao boi script test"
        ManagerId = [int]$adminId
        StartDate = $now.ToString("yyyy-MM-dd")
        EndDate   = $now.AddMonths(1).ToString("yyyy-MM-dd")
    } | ConvertTo-Json
    Invoke-RestMethod -Uri "$baseUrl/projects" -Method Post -Body $body -ContentType "application/json" -Headers $headers
}

# 4. TAO TASK (gan cho chinh Admin de test luong cham cong khong can doi token)
$task = $null
if ($project) {
    $task = Step "4. Tao Task, gan cho chinh Admin" {
        $body = @{
            ProjectId = $project.projectId; Name = "Task Test"; Description = "Tao boi script test"
            AssigneeId = [int]$adminId; EstimatedTime = 8
            StartDate = $now.ToString("yyyy-MM-dd")
            DueDate   = $now.AddDays(3).ToString("yyyy-MM-dd")
        } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/tasks" -Method Post -Body $body -ContentType "application/json" -Headers $headers
    }
}

# 5. NOP LOG-TIME
$logTime = $null
if ($task) {
    $logTime = Step "5. Nop Log-time cho Task" {
        $body = @{ TaskId = $task.taskId; ActualHours = 6; Description = "Test log"; LogDate = $now.ToString("yyyy-MM-dd") } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/logtimes" -Method Post -Body $body -ContentType "application/json" -Headers $headers
    }
}

# 6. DUYET LOG-TIME (voi quyen Admin)
if ($logTime) {
    Step "6. Duyet Log-time" {
        $body = @{ Status = "Approved" } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/logtimes/$($logTime.logId)/approve" -Method Patch -Body $body -ContentType "application/json" -Headers $headers
    }
}

# 7. CHUYEN TASK SANG DONE
if ($task) {
    Step "7. Chuyen Task sang Done" {
        $body = @{ Status = "Done" } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/tasks/$($task.taskId)/status" -Method Patch -Body $body -ContentType "application/json" -Headers $headers
    }
}

# 8. XIN NGHI PHEP
$leave = Step "8. Gui don xin nghi phep" {
    $body = @{ LeaveDate = $now.AddDays(5).ToString("yyyy-MM-dd"); Reason = "Test nghi phep" } | ConvertTo-Json
    Invoke-RestMethod -Uri "$baseUrl/leavedays" -Method Post -Body $body -ContentType "application/json" -Headers $headers
}

# 9. DUYET NGHI PHEP
if ($leave) {
    Step "9. Duyet don nghi phep" {
        $body = @{ Status = "Approved" } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/leavedays/$($leave.leaveId)/approve" -Method Patch -Body $body -ContentType "application/json" -Headers $headers
    }
}

# 10-12. XEM 3 CHI SO KPI CUA THANG HIEN TAI
Step "10. Xem KPI Tien do" {
    Invoke-RestMethod -Uri "$baseUrl/kpis/timeliness/$adminId`?month=$($now.Month)&year=$($now.Year)" -Method Get -Headers $headers
}
Step "11. Xem KPI Hieu suat" {
    Invoke-RestMethod -Uri "$baseUrl/kpis/efficiency/$adminId`?month=$($now.Month)&year=$($now.Year)" -Method Get -Headers $headers
}
Step "12. Xem KPI Khoi luong" {
    Invoke-RestMethod -Uri "$baseUrl/kpis/capacity/$adminId`?month=$($now.Month)&year=$($now.Year)" -Method Get -Headers $headers
}

# 13. CHOT KPI CUOI THANG (Tu dong dung SystemConfig vua tao o Buoc 2.2)
Step "13. Chot KPI cuoi thang (Finalize)" {
    $body = @{ ManagerScore = 90; Note = "Test tu dong chot KPI" } | ConvertTo-Json
    Invoke-RestMethod -Uri "$baseUrl/kpis/finalize/$adminId`?month=$($now.Month)&year=$($now.Year)" -Method Post -Body $body -ContentType "application/json" -Headers $headers
}

Write-Host "`n============================================" -ForegroundColor Yellow
Write-Host " KET QUA: $pass thanh cong / $fail that bai" -ForegroundColor Yellow
Write-Host "============================================" -ForegroundColor Yellow