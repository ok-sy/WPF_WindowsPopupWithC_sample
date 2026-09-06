$ErrorActionPreference = "Stop"

# 이 스크립트는 인터넷이 되는 Windows PC에서 실행한다.
# WPF 프로젝트 복원에 필요한 WebView2와 win-x64 Runtime Pack을 내려받고,
# 각 패키지의 원본 .nupkg를 저장소의 offline-packages/nuget 폴더에 모은다.

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repositoryRoot "popup-frameWork\Popup.slnx"
$downloadCache = Join-Path $repositoryRoot ".offline-cache\online-download"
$verifyCache = Join-Path $repositoryRoot ".offline-verify\packages"
$offlineFeed = Join-Path $repositoryRoot "offline-packages\nuget"
$offlineConfig = Join-Path $repositoryRoot "NuGet.config"

if (-not (Test-Path $solutionPath)) {
    throw "WPF 솔루션을 찾을 수 없습니다: $solutionPath"
}

$sdkVersion = (& dotnet --version).Trim()
if ($sdkVersion -ne "10.0.100") {
    throw ".NET SDK 10.0.100이 필요합니다. 현재 버전: $sdkVersion"
}

New-Item -ItemType Directory -Force -Path $downloadCache | Out-Null
New-Item -ItemType Directory -Force -Path $verifyCache | Out-Null
New-Item -ItemType Directory -Force -Path $offlineFeed | Out-Null

Write-Host "[1/3] 온라인 NuGet에서 win-x64 복원"
dotnet restore $solutionPath `
    --runtime win-x64 `
    --packages $downloadCache `
    --source "https://api.nuget.org/v3/index.json" `
    --force

Write-Host "[2/3] 원본 .nupkg를 저장소 내부 피드로 수집"
$packages = Get-ChildItem $downloadCache -Recurse -File -Filter "*.nupkg"
if ($packages.Count -eq 0) {
    throw "수집할 .nupkg 파일이 없습니다. restore 결과를 확인하세요."
}

foreach ($package in $packages) {
    Copy-Item $package.FullName `
        (Join-Path $offlineFeed $package.Name) `
        -Force
}

Write-Host "[3/3] 인터넷 없이 다시 restore하여 패키지 완전성 검증"
dotnet restore $solutionPath `
    --runtime win-x64 `
    --packages $verifyCache `
    --configfile $offlineConfig `
    --force

Write-Host "완료: $($packages.Count)개 패키지를 수집하고 오프라인 복원을 검증했습니다."
Write-Host "다음으로 offline-packages/nuget의 .nupkg 파일을 Git에 추가하세요."
