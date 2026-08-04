#Requires -Version 5.1
<#
.SYNOPSIS
Writes a panel/v1 file describing the real state of a git repository, so the widget
renderer can be judged against real data rather than fixtures.

.PARAMETER Repo
Repository to describe. Defaults to the AgentDeck repo this script lives in.
#>
[CmdletBinding()]
param(
    [string]$Repo = (Split-Path $PSScriptRoot -Parent)
)

$ErrorActionPreference = 'Stop'

function Invoke-Git {
    param([Parameter(ValueFromRemainingArguments)]$Arguments)
    & git.exe -C $Repo @Arguments
}

$branch = (Invoke-Git rev-parse --abbrev-ref HEAD).Trim()
$head = (Invoke-Git log -1 --format=%h).Trim()
$subject = (Invoke-Git log -1 --format=%s).Trim()
$author = (Invoke-Git log -1 --format=%an).Trim()
$dirty = @(Invoke-Git status --porcelain)

$commits = @(Invoke-Git log -12 --format='%h%x1f%an%x1f%ad%x1f%s' --date=format:'%d-%m %H:%M') | ForEach-Object {
    $parts = $_ -split "`u{1f}"
    , @($parts[0], $parts[1], $parts[2], $parts[3])
}

$diffLines = @(Invoke-Git show HEAD --unified=2 --format='') | Select-Object -First 60 | ForEach-Object {
    $text = $_
    $kind = 'context'

    if ($text.StartsWith('@@')) { $kind = 'hunk' }
    elseif ($text.StartsWith('+++') -or $text.StartsWith('---')) { $kind = 'context' }
    elseif ($text.StartsWith('+')) { $kind = 'add' }
    elseif ($text.StartsWith('-')) { $kind = 'delete' }

    @{ kind = $kind; text = $text }
}

$days = 0..6 | ForEach-Object { (Get-Date).AddDays(-6 + $_) }
$counts = $days | ForEach-Object {
    $day = $_.ToString('yyyy-MM-dd')
    [double](@(Invoke-Git log --format=%h --since="$day 00:00" --until="$day 23:59").Count)
}

if ($dirty.Count -eq 0) {
    $state = 'idle'
    $stateText = "Working tree clean on $branch"
}
else {
    $state = 'working'
    $stateText = "$($dirty.Count) uncommitted change(s) on $branch"
}

$entries = @(Invoke-Git log -6 --format='%h%x1f%s') | ForEach-Object {
    $parts = $_ -split "`u{1f}"
    $level = if ($parts[1] -match '(?i)fix|crash|revert') { 'warn' } else { 'info' }
    @{ level = $level; time = $parts[0]; text = $parts[1] }
}

$panel = [ordered]@{
    schema = 'panel/v1'
    title  = "$(Split-Path $Repo -Leaf) repository"
    blocks = @(
        @{ type = 'status'; state = $state; text = $stateText },
        @{ type = 'keyvalue'; rows = @(
            @{ key = 'Branch'; value = $branch },
            @{ key = 'HEAD'; value = "$head $subject" },
            @{ key = 'Author'; value = $author },
            @{ key = 'Dirty files'; value = "$($dirty.Count)" }
        ) },
        @{ type = 'chart'; kind = 'bar'
           labels = @($days | ForEach-Object { $_.ToString('ddd') })
           series = @(@{ label = 'Commits per day'; points = $counts }) },
        @{ type = 'table'
           columns = @('Commit', 'Author', 'When', 'Subject')
           rows = $commits },
        @{ type = 'log'; entries = $entries },
        @{ type = 'diff'; file = "HEAD at $head"; lines = $diffLines },
        @{ type = 'actions'; buttons = @(
            @{ label = 'Review HEAD'; prompt = "Review commit $head and tell me what it changes."; target = 'live' }
        ) }
    )
}

$json = $panel | ConvertTo-Json -Depth 8
$workspaces = Get-ChildItem -Path (Join-Path $env:USERPROFILE '.agentdeck\workspaces') -Directory

foreach ($workspace in $workspaces) {
    $panels = Join-Path $workspace.FullName 'panels'

    if (-not (Test-Path $panels)) {
        New-Item -ItemType Directory -Path $panels -Force | Out-Null
    }

    $destination = Join-Path $panels 'repo.json'
    [System.IO.File]::WriteAllText($destination, $json)
    Write-Host "wrote $destination"
}
