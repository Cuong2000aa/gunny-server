using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Tank.Request
{
    /// <summary>
    /// Dig-FX sanitizer via Python/Pillow (System.Drawing/GDI+ is unavailable on macOS).
    /// Crater: keep alpha for ERASE, whiten RGB. Brink: fully transparent (no brown stamp).
    /// Never touch map dead.png — that layer is required for client collision.
    /// </summary>
    internal static class DigAssetSanitizer
    {
        private static readonly ConcurrentDictionary<string, byte[]> Cache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object ScriptLock = new();
        private static string _scriptPath;

        public static bool TrySanitize(string normalizedPath, byte[] input, out byte[] output)
        {
            output = input;
            if (input == null || input.Length < 32 || !IsDigPath(normalizedPath))
            {
                return false;
            }

            string cacheKey = normalizedPath + "|" + input.Length + "|" + Fnv1a(input);
            if (Cache.TryGetValue(cacheKey, out byte[] cached))
            {
                output = cached;
                return true;
            }

            if (!TryRunPython(normalizedPath, input, out byte[] sanitized) || sanitized == null || sanitized.Length < 32)
            {
                return false;
            }

            Cache[cacheKey] = sanitized;
            output = sanitized;
            return true;
        }

        public static bool TryLoadSanitized(string normalizedPath, string filePath, out byte[] output)
        {
            output = Array.Empty<byte>();
            try
            {
                byte[] raw = File.ReadAllBytes(filePath);
                if (!TrySanitize(normalizedPath, raw, out output))
                {
                    output = raw;
                }
                return output != null && output.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsDigPath(string normalizedPath)
        {
            bool isCrater = normalizedPath.Contains("/bomb/crater/", StringComparison.OrdinalIgnoreCase)
                            && normalizedPath.EndsWith("crater.png", StringComparison.OrdinalIgnoreCase)
                            && !normalizedPath.Contains("craterbrink", StringComparison.OrdinalIgnoreCase)
                            && !normalizedPath.Contains("craterBrink", StringComparison.OrdinalIgnoreCase);
            bool isBrink = normalizedPath.Contains("/bomb/crater/", StringComparison.OrdinalIgnoreCase)
                           && (normalizedPath.EndsWith("/craterbrink.png", StringComparison.OrdinalIgnoreCase)
                               || normalizedPath.EndsWith("/craterBrink.png", StringComparison.OrdinalIgnoreCase));
            return isCrater || isBrink;
        }

        private static bool TryRunPython(string normalizedPath, byte[] input, out byte[] output)
        {
            output = null;
            try
            {
                string script = EnsureScript();
                string mode = "crater";
                if (normalizedPath.Contains("craterbrink", StringComparison.OrdinalIgnoreCase)
                    || normalizedPath.Contains("craterBrink", StringComparison.OrdinalIgnoreCase))
                {
                    mode = "brink";
                }

                string inFile = Path.Combine(Path.GetTempPath(), "gunny-dig-in-" + Guid.NewGuid().ToString("N") + ".png");
                string outFile = Path.Combine(Path.GetTempPath(), "gunny-dig-out-" + Guid.NewGuid().ToString("N") + ".png");
                File.WriteAllBytes(inFile, input);
                try
                {
                    // Win thường chỉ có `python`; Mac/Linux có `python3`.
                    foreach (string py in new[] { "python3", "python" })
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = py,
                            ArgumentList = { script, mode, inFile, outFile },
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        try
                        {
                            using var proc = Process.Start(psi);
                            if (proc == null)
                            {
                                continue;
                            }
                            if (!proc.WaitForExit(8000))
                            {
                                try { proc.Kill(true); } catch { }
                                continue;
                            }
                            if (proc.ExitCode != 0 || !File.Exists(outFile))
                            {
                                string err = proc.StandardError.ReadToEnd();
                                if (!string.IsNullOrWhiteSpace(err))
                                {
                                    Console.WriteLine($"[DigAssetSanitizer] {py} fail: {err.Trim()}");
                                }
                                continue;
                            }
                            output = File.ReadAllBytes(outFile);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[DigAssetSanitizer] {py}: {ex.Message}");
                        }
                    }
                    return false;
                }
                finally
                {
                    try { File.Delete(inFile); } catch { }
                    try { File.Delete(outFile); } catch { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DigAssetSanitizer] skip {normalizedPath}: {ex.Message}");
                return false;
            }
        }

        private static string EnsureScript()
        {
            if (!string.IsNullOrEmpty(_scriptPath) && File.Exists(_scriptPath))
            {
                return _scriptPath;
            }
            lock (ScriptLock)
            {
                if (!string.IsNullOrEmpty(_scriptPath) && File.Exists(_scriptPath))
                {
                    return _scriptPath;
                }
                string path = Path.Combine(Path.GetTempPath(), "gunny_dig_sanitize.py");
                File.WriteAllText(path, ScriptSource, Encoding.UTF8);
                _scriptPath = path;
                return _scriptPath;
            }
        }

        private static uint Fnv1a(byte[] data)
        {
            unchecked
            {
                uint hash = 2166136261;
                int step = Math.Max(1, data.Length / 256);
                for (int i = 0; i < data.Length; i += step)
                {
                    hash ^= data[i];
                    hash *= 16777619;
                }
                hash ^= (uint)data.Length;
                return hash;
            }
        }

        private const string ScriptSource = @"
import sys
from PIL import Image

mode, src, dst = sys.argv[1], sys.argv[2], sys.argv[3]
im = Image.open(src).convert('RGBA')
px = im.load()
w, h = im.size
cx, cy = w/2.0, h/2.0

# Dig cutouts only — never modify map dead.png (collision).
if mode == 'crater':
    for y in range(h):
        for x in range(w):
            r,g,b,a = px[x,y]
            if a < 8:
                px[x,y] = (0,0,0,0)
            else:
                px[x,y] = (255,255,255,a)
elif mode == 'brink':
    for y in range(h):
        for x in range(w):
            px[x,y] = (0,0,0,0)
else:
    raise SystemExit('bad mode')

im.save(dst)
";
    }
}
