$snippet = "        try`r`n        {`r`n            var summary = await ApiClient.GetAsync<FieldLifecycleSummary>(`"/api/field/current/summary`");`r`n            if (summary != null)`r`n                _fieldName = summary.FieldName ?? _fieldName;`r`n        }`r`n        catch { /* field name is non-critical */ }`r`n`r`n"

$pages = Get-ChildItem "C:\Users\f_ald\source\repos\The-Tech-Idea\Beep.OilandGas\Beep.OilandGas.Web\Pages" -Recurse -Filter "*.razor" | Where-Object {
    $c = [System.IO.File]::ReadAllText($_.FullName)
    $c -match '_fieldName = "Current Field"' -and
    $c -match 'OnInitializedAsync' -and
    -not ($c -match 'FieldName.*_fieldName|_fieldName.*=.*FieldName|summary.*_fieldName')
}

$count = 0
foreach ($p in $pages) {
    $content = [System.IO.File]::ReadAllText($p.FullName)
    $pattern = '(protected\s+override\s+async\s+Task\s+OnInitializedAsync\(\)[ \t]*[\r\n]+[ \t]*\{[ \t]*[\r\n]+)'
    if ($content -match $pattern) {
        $newContent = [regex]::Replace($content, $pattern, "`$1$snippet")
        [System.IO.File]::WriteAllText($p.FullName, $newContent, [System.Text.Encoding]::UTF8)
        $count++
        Write-Host "Fixed: $($p.Name)"
    } else {
        Write-Host "SKIPPED (pattern not found): $($p.Name)"
    }
}
Write-Host "`nTotal fixed: $count"
