$ErrorActionPreference = "Stop"

# 이 스크립트는 폐쇄망 Windows 개발 PC에서 실행한다.
# NuGet.config가 외부 NuGet 서버를 모두 제거하므로 인터넷 통신 없이 복원·게시한다.

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repositoryRoot "popup-frameWork\Popup.slnx"
$projectPath = Join-Path $repositoryRoot "popup-frameWork\Popup\Popup.csproj"
$offlineConfig = Join-Path $repositoryRoot "NuGet.config"
$packageCache = Join-Path $repositoryRoot ".offline-cache\packages"
$publishPath = Join-Path $repositoryRoot "popup-frameWork\publish\win-x64"

$sdkVersion = (& dotnet --version).Trim()
if ($sdkVersion -ne "10.0.100") {
    throw ".NET SDK 10.0.100이 필요합니다. 현재 버전: $sdkVersion"
}

Write-Host "[1/2] 저장소 내부 NuGet 패키지로 복원"
dotnet restore $solutionPath `
    --runtime win-x64 `
    --packages $packageCache `
    --configfile $offlineConfig `
    --force

Write-Host "[2/2] win-x64 Self-contained 게시"
dotnet publish $projectPath `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --no-restore `
    --output $publishPath

Write-Host "게시 완료: $publishPath"
