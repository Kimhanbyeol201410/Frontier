# Godot 4.x CLI로 sts2-frontier.pck 생성 (프로젝트 루트에 출력).
# 사용 전: 환경 변수 GODOT 을 Godot 실행 파일 전체 경로로 설정한다.
#   예: $env:GODOT = "C:\Godot\Godot_v4.5-stable_mono_win64\Godot_v4.5-stable_mono_win64_console.exe"
$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$OutPck = Join-Path $ProjectRoot "sts2-frontier.pck"
$Godot = $env:GODOT
if (-not $Godot -or -not (Test-Path -LiteralPath $Godot)) {
    Write-Error "GODOT 환경 변수를 Godot 실행 파일(.exe) 경로로 설정한 뒤 다시 실행하세요."
}
& $Godot --path $ProjectRoot --headless --export-pack "sts2-frontier-pck" $OutPck
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
Write-Host "생성됨: $OutPck"
