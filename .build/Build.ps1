param (
    [Parameter(Mandatory=$true)]
    [string]
    $ServerUrl
)

$ErrorActionPreference = "Stop"

function Replace-LineInFile($FilePath, $MatchPattern, $ReplaceLineWith, $MaxCount = -1){
    [string[]]$Content = Get-Content -Path $FilePath
    $Count = 0
    for ($i = 0; $i -lt $Content.Length; $i++)
    {
        if ($Content[$i] -ne $null -and $Content[$i].Contains($MatchPattern)) {
            $Content[$i] = $ReplaceLineWith
            $Count++
        }
        if ($MaxCount -gt 0 -and $Count -ge $MaxCount) {
            break
        }
    }
    ($Content | Out-String).Trim() | Out-File -FilePath $FilePath -Force -Encoding utf8
}

$ServerUrl = $ServerUrl.TrimEnd("/")
$InstallerDir = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer"
$VsWhere = "$InstallerDir\vswhere.exe"
$MSBuildPath = (&"$VsWhere" -latest -products * -find "\MSBuild\Current\Bin\MSBuild.exe").Trim()
$Root = (Get-Item -Path $PSScriptRoot).Parent.FullName
$DownloadsFolder = "$Root\Medior.Web\Server\wwwroot\downloads\"
$PubxmlPath = "$Root\Medior\Properties\PublishProfiles\Prod.pubxml"

Set-Location $Root

if (!(Test-Path -Path "$Root\Medior.sln")) {
    Write-Host "Unable to determine solution directory." -ForegroundColor Red
    return
}


[string[]]$Content = Get-Content -Path $PubxmlPath
$Line = $Content | Where-Object { $_.Contains("<ApplicationVersion>") }
$VersionString = $Line.Trim().Replace("<ApplicationVersion>", "").Replace("</ApplicationVersion>", "")
$CurrentVersion = [Version]::Parse($VersionString)
$NewVersion = [Version]::new($CurrentVersion.Major, $CurrentVersion.Minor, $CurrentVersion.Build + 1, 0)
Replace-LineInFile -FilePath $PubxmlPath -MatchPattern "<ApplicationVersion>" -ReplaceLineWith "    <ApplicationVersion>$NewVersion</ApplicationVersion>"

if ($ServerUrl -notlike "https://medior.jaredg.dev") {
    Replace-LineInFile -FilePath $PubxmlPath -MatchPattern "<InstallUrl>" -ReplaceLineWith "      <InstallUrl>$ServerUrl/downloads/</InstallUrl>"
    Replace-LineInFile -FilePath "$Root\Medior\Services\Settings.cs" -MatchPattern "private readonly string _defaultServerUrl" -ReplaceLineWith "private readonly string _defaultServerUrl = `"$ServerUrl`";" -MaxCount 1
}

&"$MSBuildPath" "$Root\Medior" -t:Restore -t:Publish -p:PublishProfile=Prod -p:ApplicationVersion=$NewVersion -p:Version=$NewVersion -p:FileVersion=$NewVersion -p:Configuration=Release -p:PublishDir="$DownloadsFolder"

if (Test-Path -Path "$DownloadsFolder\MediorSetup.exe") {
    Remove-Item -Path "$DownloadsFolder\MediorSetup.exe" -Force -ErrorAction SilentlyContinue
}

if (Test-Path -Path "$DownloadsFolder\setup.exe") {
    Rename-Item -Path "$DownloadsFolder\setup.exe" -NewName "MediorSetup.exe"
}

Get-ChildItem -Path "$Root\Medior.Web\Server\bin\publish" | ForEach-Object {
    Remove-Item -Path $_.FullName -Recurse -Force
}

dotnet publish -p:ExcludeApp_Data=true --runtime linux-x64 --configuration Release --output "$Root\Medior.Web\Server\bin\publish" --self-contained true "$Root\Medior.Web\Server\"