[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (& git rev-parse --show-toplevel).Trim()
if (-not $repoRoot) {
    throw 'Run this script inside the Majlis Git repository.'
}

Push-Location -LiteralPath $repoRoot
try {
    $errors = [System.Collections.Generic.List[string]]::new()

    $requiredSpecFiles = Get-ChildItem -LiteralPath 'specs' -Directory |
        ForEach-Object {
            @(
                (Join-Path $_.FullName 'spec.md'),
                (Join-Path $_.FullName 'plan.md'),
                (Join-Path $_.FullName 'tasks.md')
            )
        }

    foreach ($requiredFile in $requiredSpecFiles) {
        if (-not (Test-Path -LiteralPath $requiredFile -PathType Leaf)) {
            $errors.Add("Missing spec triplet file: $requiredFile")
        }
    }

    $requirementPattern = '\b(?:REL|DLY|ATT|PROG|COM|AUTH|LDB|SHR|NTF|ADM|MOD|OPS)-\d{3}\b'
    $specIds = Get-ChildItem -LiteralPath 'specs' -Filter 'spec.md' -Recurse |
        ForEach-Object { [regex]::Matches((Get-Content -LiteralPath $_.FullName -Raw), $requirementPattern).Value } |
        Sort-Object -Unique
    $traceability = Get-Content -LiteralPath 'docs/quality/requirements-to-tests.md' -Raw

    foreach ($requirementId in $specIds) {
        if ($traceability -notmatch "\b$([regex]::Escape($requirementId))\b") {
            $errors.Add("Requirement missing from traceability matrix: $requirementId")
        }
    }

    $duplicateIds = Get-ChildItem -LiteralPath 'specs' -Filter 'spec.md' -Recurse |
        ForEach-Object { [regex]::Matches((Get-Content -LiteralPath $_.FullName -Raw), $requirementPattern).Value } |
        Group-Object |
        Where-Object Count -gt 1
    foreach ($duplicateId in $duplicateIds) {
        $errors.Add("Requirement id is defined more than once: $($duplicateId.Name)")
    }

    $markdownFiles = Get-ChildItem -File -Recurse -Filter '*.md' |
        Where-Object { $_.FullName -notmatch '[\\/]\.git[\\/]' }
    $linkPattern = '(?<!!)\[[^\]]+\]\((?<target>[^)#]+)(?:#[^)]+)?\)'
    foreach ($markdownFile in $markdownFiles) {
        $content = Get-Content -LiteralPath $markdownFile.FullName -Raw
        $fenceCount = ([regex]::Matches($content, '(?m)^```')).Count
        if (($fenceCount % 2) -ne 0) {
            $relativeSource = $markdownFile.FullName.Substring($repoRoot.Length + 1)
            $errors.Add("Unbalanced Markdown code fences in $relativeSource")
        }

        foreach ($match in [regex]::Matches($content, $linkPattern)) {
            $target = $match.Groups['target'].Value.Trim().Trim('<', '>')
            if ($target -match '^(?:https?:|mailto:|#)') {
                continue
            }

            $resolved = Join-Path $markdownFile.DirectoryName $target
            if (-not (Test-Path -LiteralPath $resolved)) {
                $relativeSource = $markdownFile.FullName.Substring($repoRoot.Length + 1)
                $errors.Add("Broken Markdown link in $relativeSource -> $target")
            }
        }

        $repoPathPattern = '`(?<target>(?:docs|specs|\.specify|\.github|scripts)/[^`\s]+|(?:AGENTS|README|MANIFEST)\.md)`'
        foreach ($match in [regex]::Matches($content, $repoPathPattern)) {
            $target = $match.Groups['target'].Value.TrimEnd('.', ',', ':')
            if ($target.Contains('<') -or $target.Contains('*')) {
                continue
            }

            $resolved = Join-Path $repoRoot $target
            if (-not (Test-Path -LiteralPath $resolved)) {
                $relativeSource = $markdownFile.FullName.Substring($repoRoot.Length + 1)
                $errors.Add("Broken repository path in $relativeSource -> $target")
            }
        }
    }

    $trackedAndPending = & git ls-files --cached --others --exclude-standard |
        Where-Object { $_ -and $_ -notmatch '^(?:bin|obj|\.codex-temp)/' } |
        Sort-Object -Unique
    $manifestEntries = Get-Content -LiteralPath 'MANIFEST.md' |
        ForEach-Object {
            if ($_ -match '^- `([^`]+)`$') {
                $Matches[1]
            }
        } |
        Sort-Object -Unique
    $manifestDiff = Compare-Object -ReferenceObject $trackedAndPending -DifferenceObject $manifestEntries
    foreach ($difference in $manifestDiff) {
        $direction = if ($difference.SideIndicator -eq '<=') { 'missing from manifest' } else { 'not present in repository' }
        $errors.Add("Manifest entry ${direction}: $($difference.InputObject)")
    }

    $stalePatterns = @(
        'Arabic UI Later',
        'Integration tests for API endpoints later',
        'unless explicitly marked as segmented regional content',
        'Implement the next unchecked task from specs/001-playable-daily-majlis/tasks.md'
    )
    foreach ($stalePattern in $stalePatterns) {
        $matches = & rg -n --glob '*.md' --fixed-strings -- $stalePattern
        if ($LASTEXITCODE -eq 0) {
            $errors.Add("Stale documentation phrase found: $stalePattern`n$($matches -join "`n")")
        }
        elseif ($LASTEXITCODE -ne 1) {
            $errors.Add("Documentation search failed for: $stalePattern")
        }
    }

    if ($errors.Count -gt 0) {
        $errors | ForEach-Object { Write-Error $_ }
        exit 1
    }

    Write-Host "Documentation validation passed: $($markdownFiles.Count) Markdown files, $($specIds.Count) requirement ids."
}
finally {
    Pop-Location
}
