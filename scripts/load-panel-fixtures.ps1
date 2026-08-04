#Requires -Version 5.1
<#
.SYNOPSIS
Copies fixtures/panels/*.json into every AgentDeck workspace so the widget renderer
can be exercised without an agent.

.PARAMETER Remove
Deletes the fixture panels from every workspace instead of copying them.
#>
[CmdletBinding()]
param(
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

$fixtures = Join-Path (Split-Path $PSScriptRoot -Parent) 'fixtures\panels'
$workspaces = Join-Path $env:USERPROFILE '.agentdeck\workspaces'

if (-not (Test-Path $workspaces)) {
    Write-Error "No workspaces yet at $workspaces - run AgentDeck once first."
}

$names = Get-ChildItem -Path $fixtures -Filter *.json | Select-Object -ExpandProperty Name
$targets = Get-ChildItem -Path $workspaces -Directory

foreach ($workspace in $targets) {
    $panels = Join-Path $workspace.FullName 'panels'

    if (-not (Test-Path $panels)) {
        New-Item -ItemType Directory -Path $panels -Force | Out-Null
    }

    foreach ($name in $names) {
        $destination = Join-Path $panels $name

        if ($Remove) {
            if (Test-Path $destination) {
                Remove-Item $destination -Force
                Write-Host "removed $destination"
            }

            continue
        }

        Copy-Item -Path (Join-Path $fixtures $name) -Destination $destination -Force
        Write-Host "wrote $destination"
    }
}
