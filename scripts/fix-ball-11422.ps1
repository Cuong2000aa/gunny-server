# Ball 11422 (pet skill "Hoi Tho Bang Gia") has BombPartical NULL -> client waits for blastOutMovie0 forever
# -> battle loading never reaches 100% -> 60s timeout kick. Point it at 11423's explosion (bullet11423.swf exists).
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$xmlPath = Join-Path $root 'Tank.Request\BallList.xml'

docker exec gunny-sql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Gunny@2026Mac" -W -Q "SET NOCOUNT ON; UPDATE Project_Game34.dbo.Ball SET BombPartical = 11423 WHERE ID = 11422 AND ISNULL(BombPartical,0) = 0; SELECT ID, BombPartical, Crater FROM Project_Game34.dbo.Ball WHERE ID IN (11422,11423)"

$b = [IO.File]::ReadAllBytes($xmlPath)
$ms = New-Object IO.MemoryStream(, $b, 2, ($b.Length - 2))
$x = (New-Object IO.StreamReader((New-Object IO.Compression.DeflateStream($ms, [IO.Compression.CompressionMode]::Decompress)), [Text.Encoding]::UTF8)).ReadToEnd()

$m = [regex]::Match($x, '<Item [^>]*\bID="11422"[^>]*>')
if (-not $m.Success) { throw 'Ball 11422 not found in BallList.xml' }
"BEFORE: $($m.Value)"
$fixed = [regex]::Replace($m.Value, '\b(BombPartical|BlastOutID|blastOutID)="(0|)"', '$1="11423"')
if ($fixed -eq $m.Value) { 'BallList.xml already OK, nothing changed'; return }
"AFTER:  $fixed"
$x = $x.Remove($m.Index, $m.Length).Insert($m.Index, $fixed)

Copy-Item $xmlPath "$xmlPath.bak_ball11422" -Force
$raw = [Text.Encoding]::UTF8.GetBytes($x)
$out = New-Object IO.MemoryStream
$out.WriteByte(0x78); $out.WriteByte(0x9C)
$ds = New-Object IO.Compression.DeflateStream($out, [IO.Compression.CompressionLevel]::Optimal, $true)
$ds.Write($raw, 0, $raw.Length); $ds.Dispose()
[uint32]$a = 1; [uint32]$c = 0
foreach ($byte in $raw) { $a = ($a + $byte) % 65521; $c = ($c + $a) % 65521 }
$adler = ([uint64]$c -shl 16) -bor $a
foreach ($s in 24, 16, 8, 0) { $out.WriteByte([byte](($adler -shr $s) -band 0xFF)) }
[IO.File]::WriteAllBytes($xmlPath, $out.ToArray())
"Wrote $xmlPath ($($out.Length) bytes). Restart Fighting + Road, then Ctrl+F5 the game page."
