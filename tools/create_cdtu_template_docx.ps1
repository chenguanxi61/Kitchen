param(
    [Parameter(Mandatory = $true)]
    [string]$TemplateDocx,

    [Parameter(Mandatory = $true)]
    [string]$SourceMarkdown,

    [Parameter(Mandatory = $true)]
    [string]$OutputDocx
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Escape-Xml {
    param([string]$Text)
    if ($null -eq $Text) { return "" }
    return $Text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace('"', "&quot;").Replace("'", "&apos;")
}

function New-StyledParagraph {
    param(
        [string]$Text,
        [string]$StyleId,
        [string]$Color = ""
    )
    $escaped = Escape-Xml $Text
    $colorXml = ""
    if ($Color -ne "") {
        $colorXml = "<w:color w:val=`"$Color`"/>"
    }
    return "<w:p><w:pPr><w:pStyle w:val=`"$StyleId`"/></w:pPr><w:r><w:rPr>$colorXml</w:rPr><w:t xml:space=`"preserve`">$escaped</w:t></w:r></w:p>"
}

function New-RawParagraph {
    param(
        [string]$Text,
        [int]$FontSizeHalfPoints = 24,
        [string]$EastAsiaFont = "宋体",
        [string]$AsciiFont = "Times New Roman",
        [switch]$Bold,
        [switch]$Center,
        [switch]$Underline,
        [int]$SpacingBefore = 0,
        [int]$SpacingAfter = 0
    )
    $escaped = Escape-Xml $Text
    $bXml = if ($Bold) { "<w:b/>" } else { "" }
    $jcXml = if ($Center) { "<w:jc w:val=`"center`"/>" } else { "<w:jc w:val=`"both`"/>" }
    $uXml = if ($Underline) { "<w:u w:val=`"single`"/>" } else { "" }
    return "<w:p><w:pPr><w:spacing w:before=`"$SpacingBefore`" w:after=`"$SpacingAfter`" w:line=`"360`" w:lineRule=`"auto`"/>$jcXml</w:pPr><w:r><w:rPr><w:rFonts w:ascii=`"$AsciiFont`" w:hAnsi=`"$AsciiFont`" w:eastAsia=`"$EastAsiaFont`" w:cs=`"$AsciiFont`"/>$bXml$uXml<w:sz w:val=`"$FontSizeHalfPoints`"/><w:szCs w:val=`"$FontSizeHalfPoints`"/></w:rPr><w:t xml:space=`"preserve`">$escaped</w:t></w:r></w:p>"
}

function New-PageBreakParagraph {
    return "<w:p><w:r><w:br w:type=`"page`"/></w:r></w:p>"
}

function Convert-MarkdownRowToCells {
    param([string]$Line)
    $trimmed = $Line.Trim()
    if ($trimmed.StartsWith("|")) { $trimmed = $trimmed.Substring(1) }
    if ($trimmed.EndsWith("|")) { $trimmed = $trimmed.Substring(0, $trimmed.Length - 1) }
    $parts = $trimmed.Split("|")
    $cells = New-Object System.Collections.Generic.List[string]
    foreach ($part in $parts) {
        $cells.Add($part.Trim())
    }
    return $cells
}

function New-WordTableFromMarkdown {
    param(
        [string[]]$TableLines
    )

    if ($TableLines.Count -lt 2) { return "" }

    $rows = New-Object System.Collections.Generic.List[object]
    foreach ($tableLine in $TableLines) {
        $trimmed = $tableLine.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed)) { continue }
        if ($trimmed -match '^\|?\s*[-: ]+\|') { continue }
        $rows.Add((Convert-MarkdownRowToCells -Line $trimmed))
    }

    if ($rows.Count -eq 0) { return "" }

    $columnCount = $rows[0].Count
    if ($columnCount -le 0) { return "" }

    $gridWidth = [int](9000 / $columnCount)
    $gridCols = New-Object System.Collections.Generic.List[string]
    for ($i = 0; $i -lt $columnCount; $i++) {
        $gridCols.Add("<w:gridCol w:w=`"$gridWidth`"/>")
    }

    $rowXmlList = New-Object System.Collections.Generic.List[string]
    for ($rowIndex = 0; $rowIndex -lt $rows.Count; $rowIndex++) {
        $cellXmlList = New-Object System.Collections.Generic.List[string]
        $currentRow = $rows[$rowIndex]
        for ($cellIndex = 0; $cellIndex -lt $columnCount; $cellIndex++) {
            $cellText = ""
            if ($cellIndex -lt $currentRow.Count) {
                $cellText = Escape-Xml ([string]$currentRow[$cellIndex])
            }
            $shdXml = ""
            if ($rowIndex -eq 0) {
                $shdXml = "<w:shd w:val=`"clear`" w:color=`"auto`" w:fill=`"D9EAF7`"/>"
            }
            $cellXmlList.Add(@"
<w:tc>
  <w:tcPr>
    <w:tcW w:w="$gridWidth" w:type="dxa"/>
    <w:vAlign w:val="center"/>
    $shdXml
  </w:tcPr>
  <w:p>
    <w:pPr><w:pStyle w:val="88"/><w:jc w:val="center"/></w:pPr>
    <w:r><w:t xml:space="preserve">$cellText</w:t></w:r>
  </w:p>
</w:tc>
"@)
        }
        $rowXmlList.Add("<w:tr>$($cellXmlList -join '')</w:tr>")
    }

    return @"
<w:tbl>
  <w:tblPr>
    <w:tblW w:w="9000" w:type="dxa"/>
    <w:jc w:val="center"/>
    <w:tblBorders>
      <w:top w:val="single" w:sz="8" w:space="0" w:color="000000"/>
      <w:left w:val="single" w:sz="8" w:space="0" w:color="000000"/>
      <w:bottom w:val="single" w:sz="8" w:space="0" w:color="000000"/>
      <w:right w:val="single" w:sz="8" w:space="0" w:color="000000"/>
      <w:insideH w:val="single" w:sz="8" w:space="0" w:color="000000"/>
      <w:insideV w:val="single" w:sz="8" w:space="0" w:color="000000"/>
    </w:tblBorders>
    <w:tblLayout w:type="fixed"/>
  </w:tblPr>
  <w:tblGrid>
    $($gridCols -join '')
  </w:tblGrid>
  $($rowXmlList -join "`n")
</w:tbl>
"@
}

function New-TocFieldParagraph {
    return @"
<w:p>
  <w:pPr><w:pStyle w:val="68"/></w:pPr>
  <w:fldSimple w:instr="TOC \o &quot;1-3&quot; \h \z \u">
    <w:r><w:t>请在 Word 中右键目录并选择“更新域”。</w:t></w:r>
  </w:fldSimple>
</w:p>
"@
}

function New-CoverTable {
    param([hashtable]$Fields)

    $rows = New-Object System.Collections.Generic.List[string]
    foreach ($key in $Fields.Keys) {
        $value = Escape-Xml $Fields[$key]
        $label = Escape-Xml $key
        $rows.Add(@"
<w:tr>
  <w:trPr><w:trHeight w:val="850" w:hRule="exact"/></w:trPr>
  <w:tc>
    <w:tcPr><w:tcW w:w="2202" w:type="dxa"/><w:vAlign w:val="bottom"/></w:tcPr>
    <w:p>
      <w:pPr><w:jc w:val="distribute"/><w:spacing w:line="360" w:lineRule="auto"/></w:pPr>
      <w:r><w:rPr><w:rFonts w:eastAsia="黑体" w:ascii="Times New Roman" w:hAnsi="Times New Roman"/><w:b/><w:sz w:val="32"/></w:rPr><w:t>$label</w:t></w:r>
    </w:p>
  </w:tc>
  <w:tc>
    <w:tcPr><w:tcW w:w="4394" w:type="dxa"/><w:tcBorders><w:bottom w:val="single" w:color="auto" w:sz="4" w:space="0"/></w:tcBorders><w:vAlign w:val="bottom"/></w:tcPr>
    <w:p>
      <w:pPr><w:jc w:val="left"/><w:spacing w:line="360" w:lineRule="auto"/></w:pPr>
      <w:r><w:rPr><w:rFonts w:eastAsia="黑体" w:ascii="Times New Roman" w:hAnsi="Times New Roman"/><w:b/><w:sz w:val="32"/></w:rPr><w:t>$value</w:t></w:r>
    </w:p>
  </w:tc>
</w:tr>
"@)
    }

    $rowsXml = ($rows -join "`n")
    return @"
<w:tbl>
  <w:tblPr>
    <w:tblW w:w="0" w:type="auto"/>
    <w:jc w:val="center"/>
    <w:tblBorders>
      <w:top w:val="none" w:sz="0" w:space="0" w:color="auto"/>
      <w:left w:val="none" w:sz="0" w:space="0" w:color="auto"/>
      <w:bottom w:val="single" w:sz="4" w:space="0" w:color="auto"/>
      <w:right w:val="none" w:sz="0" w:space="0" w:color="auto"/>
      <w:insideH w:val="single" w:sz="4" w:space="0" w:color="auto"/>
      <w:insideV w:val="none" w:sz="0" w:space="0" w:color="auto"/>
    </w:tblBorders>
    <w:tblLayout w:type="fixed"/>
  </w:tblPr>
  <w:tblGrid>
    <w:gridCol w:w="2202"/>
    <w:gridCol w:w="4394"/>
  </w:tblGrid>
$rowsXml
</w:tbl>
"@
}

function Get-SectionText {
    param(
        [string[]]$Lines,
        [string]$StartHeader,
        [string]$EndHeader = ""
    )
    $startIndex = -1
    for ($i = 0; $i -lt $Lines.Count; $i++) {
        if ($Lines[$i].Trim() -eq $StartHeader) {
            $startIndex = $i + 1
            break
        }
    }
    if ($startIndex -lt 0) { return @() }
    $endIndex = $Lines.Count
    if ($EndHeader -ne "") {
        for ($j = $startIndex; $j -lt $Lines.Count; $j++) {
            if ($Lines[$j].Trim() -eq $EndHeader) {
                $endIndex = $j
                break
            }
        }
    }
    return $Lines[$startIndex..($endIndex - 1)]
}

if (-not (Test-Path -LiteralPath $TemplateDocx)) { throw "Template not found: $TemplateDocx" }
if (-not (Test-Path -LiteralPath $SourceMarkdown)) { throw "Source markdown not found: $SourceMarkdown" }

$lines = Get-Content -LiteralPath $SourceMarkdown -Encoding UTF8

$title = "基于Unity的多人协作烹饪游戏设计与实现"
$cnAbstractLines = Get-SectionText -Lines $lines -StartHeader "## 摘要" -EndHeader "---"
$bodyLines = Get-SectionText -Lines $lines -StartHeader "## 第一章 绪论"

$paras = New-Object System.Collections.Generic.List[string]

# Cover
$paras.Add((New-RawParagraph -Text "本科毕业论文（设计）" -FontSizeHalfPoints 36 -EastAsiaFont "黑体" -Bold -Center -SpacingBefore 240 -SpacingAfter 240))
$paras.Add((New-RawParagraph -Text "题目：" -FontSizeHalfPoints 28 -EastAsiaFont "黑体" -Bold -Center -SpacingBefore 480 -SpacingAfter 120))
$paras.Add((New-RawParagraph -Text $title -FontSizeHalfPoints 36 -EastAsiaFont "黑体" -Bold -Center -Underline -SpacingBefore 120 -SpacingAfter 360))
$coverFields = [ordered]@{
    "学生姓名：" = "陈佳康"
    "学   号：" = "待填写"
    "班   级：" = "待填写"
    "专   业：" = "软件工程"
    "院（系）：" = "计算机工程学院"
    "指导教师：" = "待填写"
}
$paras.Add((New-CoverTable -Fields $coverFields))
$paras.Add((New-RawParagraph -Text "二〇二六年四月" -FontSizeHalfPoints 24 -EastAsiaFont "宋体" -Center -SpacingBefore 360 -SpacingAfter 0))
$paras.Add((New-PageBreakParagraph))

# Chinese abstract
$paras.Add((New-StyledParagraph -Text "摘    要" -StyleId "61"))
foreach ($line in $cnAbstractLines) {
    $trimmed = $line.Trim()
    if ([string]::IsNullOrWhiteSpace($trimmed)) { continue }
    if ($trimmed -eq "**关键词：** Unity；多人协作游戏；Netcode for GameObjects；ScriptableObject；游戏系统设计") {
        $paras.Add((New-StyledParagraph -Text "关键词：Unity；多人协作游戏；Netcode for GameObjects；ScriptableObject；游戏系统设计" -StyleId "63"))
        continue
    }
    $paras.Add((New-StyledParagraph -Text $trimmed -StyleId "62"))
}
$paras.Add((New-PageBreakParagraph))

# English abstract
$enAbstract = @(
    "ABSTRACT",
    "With the continuous development of the digital game industry and interactive software technology, game system development based on the Unity engine has become an important practical direction in software engineering and digital media education. Compared with traditional stand-alone games, multiplayer cooperative games involve higher system complexity in task allocation, real-time interaction, state synchronization and interface feedback, and are therefore suitable as comprehensive engineering practice topics.",
    "This paper designs and implements a multiplayer cooperative cooking game system based on Unity. The system uses C# as the development language, Unity 2022.3 as the runtime environment, ScriptableObject for recipe, ingredient and processing rule configuration, and Netcode for GameObjects to build a basic multiplayer networking framework. Around the core gameplay flow, the system is divided into modules for game state management, multiplayer session management, player interaction, counter processing, order and delivery management, and user interface display.",
    "At the implementation level, the project realizes character movement and interaction, cutting and cooking state transitions, order generation and delivery judgment, and basic multiplayer preparation and scene switching. Players can cooperate within a limited time to complete orders, while the system provides task and progress feedback through order lists, progress bars, countdown displays and pause interfaces. Test results show that the system can complete the single-player core gameplay loop and support basic multiplayer cooperative demonstration.",
    "The study shows that Unity and Netcode for GameObjects can be used efficiently to build a small and medium-sized multiplayer cooperative game prototype. The completed system can serve as a practical case for cooperative game development and provide references for similar projects in networking interaction, module decoupling and data-driven design. However, there is still room for improvement in network consistency, test coverage and gameplay content expansion.",
    "Key words: Unity; multiplayer cooperative game; Netcode for GameObjects; ScriptableObject; game system design"
)
$paras.Add((New-StyledParagraph -Text "ABSTRACT" -StyleId "64"))
for ($i = 1; $i -lt ($enAbstract.Count - 1); $i++) {
    $paras.Add((New-StyledParagraph -Text $enAbstract[$i] -StyleId "65"))
}
$paras.Add((New-StyledParagraph -Text $enAbstract[-1] -StyleId "66"))
$paras.Add((New-PageBreakParagraph))

# TOC
$paras.Add((New-StyledParagraph -Text "目    录" -StyleId "67"))
$paras.Add((New-TocFieldParagraph))
$paras.Add((New-PageBreakParagraph))

# Body
 $tableBuffer = New-Object System.Collections.Generic.List[string]
 $skipFigureHints = $false

foreach ($line in $bodyLines) {
    $trimmed = $line.Trim()

    if ($trimmed -eq "## 附录：插图与图表标注建议") {
        $skipFigureHints = $true
        continue
    }
    if ($trimmed -eq "## 附录：测试表可直接使用版本") {
        $skipFigureHints = $false
    }
    if ($skipFigureHints) {
        continue
    }

    if ($trimmed.StartsWith("|")) {
        $tableBuffer.Add($trimmed)
        continue
    }
    if ($tableBuffer.Count -gt 0) {
        $paras.Add((New-WordTableFromMarkdown -TableLines $tableBuffer.ToArray()))
        $tableBuffer.Clear()
    }

    if ([string]::IsNullOrWhiteSpace($trimmed)) {
        continue
    }
    if ($trimmed -eq "---") {
        continue
    }
    if ($trimmed.StartsWith("## ")) {
        $paras.Add((New-StyledParagraph -Text $trimmed.Substring(3) -StyleId "73"))
        continue
    }
    if ($trimmed.StartsWith("### ")) {
        $paras.Add((New-StyledParagraph -Text $trimmed.Substring(4) -StyleId "74"))
        continue
    }
    if ($trimmed.StartsWith("#### ")) {
        $paras.Add((New-StyledParagraph -Text $trimmed.Substring(5) -StyleId "75"))
        continue
    }
    if ($trimmed.StartsWith("[FIGURE_HINT]") -or $trimmed.StartsWith("[TABLE_HINT]")) {
        continue
    }
    $paras.Add((New-StyledParagraph -Text $trimmed -StyleId "76"))
}

if ($tableBuffer.Count -gt 0) {
    $paras.Add((New-WordTableFromMarkdown -TableLines $tableBuffer.ToArray()))
    $tableBuffer.Clear()
}

# References and acknowledgement placeholders
$paras.Add((New-PageBreakParagraph))
$paras.Add((New-StyledParagraph -Text "参考文献" -StyleId "78"))
$paras.Add((New-StyledParagraph -Text "[1] 待根据最终引用文献统一整理。" -StyleId "79"))
$paras.Add((New-StyledParagraph -Text "[2] 当前文中引用序号需在定稿阶段统一核对。" -StyleId "79"))

$paras.Add((New-PageBreakParagraph))
$paras.Add((New-StyledParagraph -Text "致    谢" -StyleId "80"))
$paras.Add((New-StyledParagraph -Text "在本次毕业论文（设计）完成过程中，感谢指导教师在选题、写作与修改中的指导与帮助；感谢学院老师在毕业设计阶段提供的支持；感谢同学和家人在项目完成过程中的鼓励与帮助。" -StyleId "81"))

$bodyXml = ($paras -join "`n")
$documentXml = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:document xmlns:wpc="http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas"
 xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
 xmlns:o="urn:schemas-microsoft-com:office:office"
 xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
 xmlns:m="http://schemas.openxmlformats.org/officeDocument/2006/math"
 xmlns:v="urn:schemas-microsoft-com:vml"
 xmlns:wp14="http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing"
 xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"
 xmlns:w10="urn:schemas-microsoft-com:office:word"
 xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"
 xmlns:w14="http://schemas.microsoft.com/office/word/2010/wordml"
 xmlns:wpg="http://schemas.microsoft.com/office/word/2010/wordprocessingGroup"
 xmlns:wpi="http://schemas.microsoft.com/office/word/2010/wordprocessingInk"
 xmlns:wne="http://schemas.microsoft.com/office/word/2006/wordml"
 xmlns:wps="http://schemas.microsoft.com/office/word/2010/wordprocessingShape"
 mc:Ignorable="w14 wp14">
  <w:body>
$bodyXml
    <w:sectPr>
      <w:pgSz w:w="11906" w:h="16838"/>
      <w:pgMar w:top="1440" w:right="1800" w:bottom="1440" w:left="1800" w:header="851" w:footer="992" w:gutter="0"/>
      <w:cols w:space="425"/>
      <w:docGrid w:type="lines" w:linePitch="312"/>
    </w:sectPr>
  </w:body>
</w:document>
"@

$outputDir = Split-Path -Parent $OutputDocx
if (-not [string]::IsNullOrWhiteSpace($outputDir) -and -not (Test-Path -LiteralPath $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

if (Test-Path -LiteralPath $OutputDocx) {
    Remove-Item -LiteralPath $OutputDocx -Force
}

Copy-Item -LiteralPath $TemplateDocx -Destination $OutputDocx -Force

$tempRoot = Join-Path $env:TEMP ("cdtu_docx_" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $tempRoot | Out-Null

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::ExtractToDirectory($OutputDocx, $tempRoot)
Set-Content -LiteralPath (Join-Path $tempRoot "word\document.xml") -Value $documentXml -Encoding utf8
Remove-Item -LiteralPath $OutputDocx -Force
[System.IO.Compression.ZipFile]::CreateFromDirectory($tempRoot, $OutputDocx)
Remove-Item -LiteralPath $tempRoot -Recurse -Force

Write-Output "CDTU_DOCX_CREATED:$OutputDocx"
