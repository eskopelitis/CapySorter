$ErrorActionPreference = 'Stop'

# Constants (project and editor paths)
$ProjectPath = 'C:\Users\anton\My project'
$UnityEditor  = if ($env:UNITY_EDITOR_PATH -and (Test-Path $env:UNITY_EDITOR_PATH)) { $env:UNITY_EDITOR_PATH } else { 'C:\Program Files\Unity\Hub\Editor\6000.2.2f1\Editor\Unity.exe' }
$ToolsDir     = Join-Path $ProjectPath 'Tools'

# Ensure Tools dir exists
if (!(Test-Path $ToolsDir)) { New-Item -ItemType Directory -Force -Path $ToolsDir | Out-Null }

function Get-UnityProcessesForProject {
  [CmdletBinding()]
  param([Parameter(Mandatory)][string]$Project)
  try {
    Get-CimInstance Win32_Process -Filter "name='Unity.exe'" -ErrorAction SilentlyContinue |
      Where-Object { $_.CommandLine -and ($_.CommandLine -match '\-projectPath') -and ($_.CommandLine -like "*$Project*") }
  } catch { @() }
}

function Stop-UnityProcessesForProject {
  [CmdletBinding()]
  param([Parameter(Mandatory)][string]$Project)
  $procs = Get-UnityProcessesForProject -Project $Project
  if ($procs) {
    Write-Host ("Stopping Unity for this project: {0} instance(s)" -f @($procs).Count)
    foreach ($p in $procs) { try { Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue } catch {} }
    Start-Sleep -Seconds 2
  }
}

function Test-UnityLockPresent {
  [CmdletBinding()]
  param([Parameter(Mandatory)][string]$Project)
  Test-Path (Join-Path $Project 'Temp/UnityLockfile')
}

function Remove-UnityLock {
  [CmdletBinding()]
  param([Parameter(Mandatory)][string]$Project)
  if (-not (Get-UnityProcessesForProject -Project $Project)) {
    $lock = Join-Path $Project 'Temp/UnityLockfile'
    if (Test-Path $lock) { Write-Host ("Removing stale lockfile: {0}" -f $lock); Remove-Item -Force $lock -ErrorAction SilentlyContinue }
  } else {
    Write-Host 'Lockfile present, but Unity is still running for this project; not removing.'
  }
}

function Wait-UnityProjectRelease {
  [CmdletBinding()]
  param([Parameter(Mandatory)][string]$Project,[int]$TimeoutSec=30)
  $deadline=(Get-Date).AddSeconds($TimeoutSec)
  do {
    $busy = $false
    if (Get-UnityProcessesForProject -Project $Project) { $busy = $true }
    if (Test-UnityLockPresent -Project $Project) { $busy = $true }
    if (-not $busy) { return $true }
    Start-Sleep -Milliseconds 800
  } while ((Get-Date) -lt $deadline)
  return $false
}

function Clear-UnityProjectLock {
  [CmdletBinding()]
  param([Parameter(Mandatory)][string]$Project,[int]$TimeoutSec=45)
  $deadline=(Get-Date).AddSeconds($TimeoutSec)
  do {
    Stop-UnityProcessesForProject -Project $Project
    if (-not (Get-UnityProcessesForProject -Project $Project)) { Remove-UnityLock -Project $Project }
    if (-not (Get-UnityProcessesForProject -Project $Project) -and -not (Test-UnityLockPresent -Project $Project)) { return }
    Start-Sleep -Milliseconds 800
  } while ((Get-Date) -lt $deadline)
}

function Resolve-UnityResultsPath {
  [CmdletBinding()]
  param([Parameter(Mandatory)][string]$Expected,[Parameter(Mandatory)][string]$Log)
  if (Test-Path $Expected) { return $Expected }
  try {
    if (Test-Path $Log) {
      $m = Select-String -Path $Log -Pattern 'Saving results to:\s*(.+TestResults\.xml)' -ErrorAction SilentlyContinue | Select-Object -Last 1
      if ($m -and $m.Matches -and $m.Matches[0].Groups.Count -gt 1) {
        $src = $m.Matches[0].Groups[1].Value.Trim()
        if (Test-Path $src) { Copy-Item -Force $src $Expected -ErrorAction SilentlyContinue; if (Test-Path $Expected) { return $Expected } }
      }
    }
    $ll = Join-Path $env:USERPROFILE 'AppData/LocalLow/DefaultCompany'
    if (Test-Path $ll) {
      $cand = Get-ChildItem -Path $ll -Recurse -Filter 'TestResults.xml' -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
      if ($cand) { Copy-Item -Force $cand.FullName $Expected -ErrorAction SilentlyContinue; if (Test-Path $Expected) { return $Expected } }
    }
  } catch {}
  return $null
}

function Invoke-UnityTests {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)][ValidateSet('EditMode','PlayMode')][string]$Platform,
    [Parameter(Mandatory)][string]$Results,
    [Parameter(Mandatory)][string]$Log,
    [string]$AssemblyNames,
    [int]$TimeoutSec = 1200
  )
  function Format-Argument { param([string]$s) if ($s -match '\s') { '"' + $s + '"' } else { $s } }
  $uArgs = @(
    '-batchmode','-nographics','-stackTraceLogType','Full',
  '-projectPath',(Format-Argument $ProjectPath),'-logFile',(Format-Argument $Log),
    '-runTests','-testPlatform',$Platform
  )
  if ($AssemblyNames) { $uArgs += @('-assemblyNames',$AssemblyNames) }
  $uArgs += @('-testResults',(Format-Argument $Results),'-testResultsFormat','nunit3')

  Write-Host ("Launching Unity {0} tests with args:\n{1}" -f $Platform, ($uArgs -join ' '))
  $proc = Start-Process -FilePath $UnityEditor -ArgumentList $uArgs -PassThru
  # Explicit wait loop for reliability on Windows PowerShell 5.1
  $deadline = (Get-Date).AddSeconds($TimeoutSec)
  while ($true) {
    $alive = $null -ne (Get-Process -Id $proc.Id -ErrorAction SilentlyContinue)
    if (-not $alive) { break }
    if ((Get-Date) -ge $deadline) {
      Write-Warning ("Unity {0} run exceeded timeout of {1}s - terminating." -f $Platform,$TimeoutSec)
      try { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue } catch {}
      break
    }
    Start-Sleep -Milliseconds 500
  }
  return $proc.ExitCode
}

function Show-NUnitSummary {
  [CmdletBinding()]
  param([Parameter(Mandatory)][string]$XmlPath,[string]$Label)
  [xml]$x = Get-Content $XmlPath
  $run = $x.'test-run'
  $total=[int]$run.total; $passed=[int]$run.passed; $failed=[int]$run.failed; $dur=[string]$run.duration
  Write-Host ("{0}: total={1} passed={2} failed={3} duration={4}s" -f $Label,$total,$passed,$failed,$dur)
  if ($run.result -ne 'Passed') {
    $fails = $x.SelectNodes("//test-case[@result!='Passed']")
    Write-Host "Failing tests:" -ForegroundColor Red
    foreach ($f in $fails) {
      $msg = $f.SelectSingleNode('.//failure/message')
      $line = if ($msg) { ($msg.InnerText -split "`n")[0] } else { '<no message>' }
      Write-Host (" - {0}: {1}" -f $f.fullname,$line) -ForegroundColor Red
    }
    return $false
  }
  return $true
}

# Paths
$EditXml = Join-Path $ToolsDir 'TestResults_EditMode.xml'
$EditLog = Join-Path $ToolsDir 'UnityEditModeTests.log'
$PlayXml = Join-Path $ToolsDir 'TestResults_PlayMode.xml'
$PlayLog = Join-Path $ToolsDir 'UnityPlayModeTests.log'

# Clean artifacts
Remove-Item -Force -ErrorAction SilentlyContinue $EditXml,$EditLog,$PlayXml,$PlayLog

Write-Host ("Using Unity: {0}" -f $UnityEditor)

# 1) EDITMODE
Write-Host "--- EDITMODE: starting headless test run ---"
Clear-UnityProjectLock -Project $ProjectPath -TimeoutSec 45
$null = Invoke-UnityTests -Platform EditMode -Results $EditXml -Log $EditLog -AssemblyNames 'EditModeTests'
Start-Sleep -Milliseconds 400
if (!(Test-Path $EditXml)) { Resolve-UnityResultsPath -Expected $EditXml -Log $EditLog | Out-Null }
if (!(Test-Path $EditXml)) { throw ("EditMode test results not found: {0}" -f $EditXml) }
$okE = Show-NUnitSummary -XmlPath $EditXml -Label 'EditMode'
if (-not $okE) { exit 1 }

# Ensure the project is fully released before PlayMode
if (-not (Wait-UnityProjectRelease -Project $ProjectPath -TimeoutSec 20)) { Clear-UnityProjectLock -Project $ProjectPath -TimeoutSec 45 }

# 2) PLAYMODE
Write-Host "`n--- PLAYMODE: starting headless test run ---"
Clear-UnityProjectLock -Project $ProjectPath -TimeoutSec 45
$null = Invoke-UnityTests -Platform PlayMode -Results $PlayXml -Log $PlayLog -AssemblyNames 'PlayModeTests'
Start-Sleep -Milliseconds 400
if (!(Test-Path $PlayXml)) { Resolve-UnityResultsPath -Expected $PlayXml -Log $PlayLog | Out-Null }

if (!(Test-Path $PlayXml)) {
  # Check for project-open crash and retry once
  $retry = $false
  if (Test-Path $PlayLog) {
  $bad = Select-String -Path $PlayLog -Pattern 'another Unity instance is running|already open' -ErrorAction SilentlyContinue
    if ($bad) { $retry = $true }
  }
  if ($retry) {
    Write-Warning 'PlayMode run failed due to project lock. Retrying once after clearing lock...'
    Clear-UnityProjectLock -Project $ProjectPath -TimeoutSec 45
    Start-Sleep -Seconds 3
  $null = Invoke-UnityTests -Platform PlayMode -Results $PlayXml -Log $PlayLog -AssemblyNames 'PlayModeTests'
    Start-Sleep -Milliseconds 400
    if (!(Test-Path $PlayXml)) { Resolve-UnityResultsPath -Expected $PlayXml -Log $PlayLog | Out-Null }
  }
}

if (!(Test-Path $PlayXml)) { throw ("PlayMode test results not found: {0}" -f $PlayXml) }
$okP = Show-NUnitSummary -XmlPath $PlayXml -Label 'PlayMode'
if (-not $okP) { exit 1 }

Write-Host "All EditMode and PlayMode tests passed. Results in Tools/*.xml and logs in Tools/*.log"
exit 0
