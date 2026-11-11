using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Bussiness;

namespace Tank.Request
{
    public class Program
    {
        private static readonly ConcurrentDictionary<string, Type> _handlers = new(StringComparer.OrdinalIgnoreCase);

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(828);
                options.AllowSynchronousIO = true;
            });

            builder.Services.AddSystemWebAdapters();
            builder.Services.AddCors();

            var app = builder.Build();

            app.UseCors(policy => policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".swf"] = "application/x-shockwave-flash";
            contentTypeProvider.Mappings[".xml"] = "text/xml";
            contentTypeProvider.Mappings[".png"] = "image/png";
            contentTypeProvider.Mappings[".jpg"] = "image/jpeg";
            contentTypeProvider.Mappings[".gif"] = "image/gif";
            contentTypeProvider.Mappings[".txt"] = "text/plain";
            contentTypeProvider.Mappings[".html"] = "text/html";
            contentTypeProvider.Mappings[".flv"] = "video/x-flv";
            contentTypeProvider.Mappings[".mp3"] = "audio/mpeg";
            contentTypeProvider.Mappings[".ttf"] = "font/ttf";
            contentTypeProvider.Mappings[".otf"] = "font/otf";
            contentTypeProvider.Mappings[".woff"] = "font/woff";
            contentTypeProvider.Mappings[".woff2"] = "font/woff2";

            string rootDir = Directory.GetParent(builder.Environment.ContentRootPath)?.FullName ?? builder.Environment.ContentRootPath;
            string flashDir = Path.Combine(rootDir, "Source Flash", "FlashSV1");
            string tankFlashDir = Path.Combine(rootDir, "Tank.Flash");
            string tankRequestDir = builder.Environment.ContentRootPath;
            string resourcesDir = Path.Combine(rootDir, "Resources");
            Directory.CreateDirectory(resourcesDir);

            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            var cdnHttpClient = new HttpClient(httpClientHandler)
            {
                Timeout = TimeSpan.FromSeconds(2)
            };
            cdnHttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 GunnyClient");

            byte[] transparentPixel = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=");

            // Asset proxy & cache middleware for /image/* and /sound/*
            app.Use(async (context, next) =>
            {
                string path = context.Request.Path.Value?.TrimStart('/') ?? "";

                if (path.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("sound/", StringComparison.OrdinalIgnoreCase))
                {
                    // Flash PathManager historically requests craterBrink.png but CDN/local
                    // assets are craterbrink.png — wrong casing made dig edges 1x1 transparent.
                    string normalizedPath = NormalizeAssetPath(path);
                    var candidatePaths = GetAssetPathCandidates(normalizedPath);

                    string localFilePath = ResolveLocalAssetPath(resourcesDir, candidatePaths);

                    // Check gender cross-check if missing (unisex items often placed in f/ or m/)
                    if (localFilePath == null)
                    {
                        if (normalizedPath.Contains("/m/", StringComparison.OrdinalIgnoreCase))
                        {
                            localFilePath = ResolveLocalAssetPath(resourcesDir, GetAssetPathCandidates(normalizedPath.Replace("/m/", "/f/", StringComparison.OrdinalIgnoreCase)));
                        }
                        else if (normalizedPath.Contains("/f/", StringComparison.OrdinalIgnoreCase))
                        {
                            localFilePath = ResolveLocalAssetPath(resourcesDir, GetAssetPathCandidates(normalizedPath.Replace("/f/", "/m/", StringComparison.OrdinalIgnoreCase)));
                        }
                    }

                    if (localFilePath != null && File.Exists(localFilePath))
                    {
                        if (contentTypeProvider.TryGetContentType(localFilePath, out string localContentType))
                        {
                            context.Response.ContentType = localContentType;
                        }
                        else if (localFilePath.EndsWith(".flv", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.ContentType = "video/x-flv";
                        }
                        else
                        {
                            context.Response.ContentType = "application/octet-stream";
                        }

                        context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                        // Dig assets already fixed on disk — serve directly (no per-request Python; that stalled/killed Request).
                        bool isDigAssetLocal = normalizedPath.Contains("/bomb/crater/", StringComparison.OrdinalIgnoreCase);
                        context.Response.Headers.Append("Cache-Control",
                            isDigAssetLocal ? "no-cache, max-age=0, must-revalidate" : "public, max-age=86400");
                        await context.Response.SendFileAsync(localFilePath);
                        return;
                    }

                    // Not cached locally -> try fetching from gunny.vcdn.vn CDN (short 2s timeout)
                    // Prefer canonical lowercase craterbrink path; keep original as fallback.
                    string cachePath = Path.Combine(resourcesDir, candidatePaths[0].Replace('/', Path.DirectorySeparatorChar));
                    try
                    {
                        HttpResponseMessage? cdnResponse = null;
                        string? hitRemotePath = null;
                        foreach (string candidate in candidatePaths)
                        {
                            string remoteUrl = $"https://gunny.vcdn.vn/{candidate}{context.Request.QueryString}";
                            cdnResponse = await cdnHttpClient.GetAsync(remoteUrl);
                            if (cdnResponse.IsSuccessStatusCode)
                            {
                                hitRemotePath = candidate;
                                break;
                            }
                            cdnResponse.Dispose();
                            cdnResponse = null;
                        }

                        if (cdnResponse != null && cdnResponse.IsSuccessStatusCode)
                        {
                            using (cdnResponse)
                            {
                                var data = await cdnResponse.Content.ReadAsByteArrayAsync();
                                bool isDigCdn = normalizedPath.Contains("/bomb/crater/", StringComparison.OrdinalIgnoreCase);
                                if (isDigCdn)
                                {
                                    DigAssetSanitizer.TrySanitize(normalizedPath, data, out data);
                                }

                                // Cache under canonical craterbrink/crater path when possible
                                string savePath = cachePath;
                                if (!string.IsNullOrEmpty(hitRemotePath))
                                {
                                    savePath = Path.Combine(resourcesDir, hitRemotePath.Replace('/', Path.DirectorySeparatorChar));
                                }

                                byte[] cacheData = data;
                                _ = Task.Run(async () =>
                                {
                                    try
                                    {
                                        string? dir = Path.GetDirectoryName(savePath);
                                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                                        {
                                            Directory.CreateDirectory(dir);
                                        }
                                        await File.WriteAllBytesAsync(savePath, cacheData);
                                    }
                                    catch {}
                                });

                                context.Response.StatusCode = 200;
                                string cType = cdnResponse.Content.Headers.ContentType?.ToString() ?? "image/png";
                                context.Response.ContentType = cType;
                                context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                                context.Response.Headers.Append("Cache-Control",
                                    isDigCdn ? "no-cache, max-age=0, must-revalidate" : "public, max-age=86400");
                                await context.Response.Body.WriteAsync(data);
                                return;
                            }
                        }

                        bool isCraterAsset = normalizedPath.Contains("/bomb/crater/", StringComparison.OrdinalIgnoreCase);
                        if (isCraterAsset)
                        {
                            // Never return 1x1 transparent for craters!
                            // Fallback to default crater 4 so digging always renders a clean hole with brink.
                            string fallbackName = (normalizedPath.EndsWith("/craterbrink.png", StringComparison.OrdinalIgnoreCase) ||
                                                   normalizedPath.EndsWith("/craterBrink.png", StringComparison.OrdinalIgnoreCase))
                                ? "craterbrink.png" : "crater.png";
                            string fallbackPath = Path.Combine(resourcesDir, "image", "bomb", "crater", "4", fallbackName);
                            if (File.Exists(fallbackPath)
                                && DigAssetSanitizer.TryLoadSanitized(
                                    "image/bomb/crater/4/" + fallbackName, fallbackPath, out byte[] fallbackBytes))
                            {
                                context.Response.StatusCode = 200;
                                context.Response.ContentType = "image/png";
                                context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                                context.Response.Headers.Append("Cache-Control", "no-cache, max-age=0, must-revalidate");
                                await context.Response.Body.WriteAsync(fallbackBytes);
                                return;
                            }
                        }
                        else if (normalizedPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        {
                            // Trigger background download to pre-cache asset for next time
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    using var bgClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                                    bgClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 GunnyClient");
                                    foreach (string c in candidatePaths)
                                    {
                                        string bgUrl = $"https://gunny.vcdn.vn/{c}";
                                        var resp = await bgClient.GetAsync(bgUrl);
                                        if (resp.IsSuccessStatusCode)
                                        {
                                            byte[] bgData = await resp.Content.ReadAsByteArrayAsync();
                                            if (bgData.Length > 50)
                                            {
                                                DigAssetSanitizer.TrySanitize(c, bgData, out bgData);
                                                string bgSave = Path.Combine(resourcesDir, c.Replace('/', Path.DirectorySeparatorChar));
                                                string? dir = Path.GetDirectoryName(bgSave);
                                                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                                                await File.WriteAllBytesAsync(bgSave, bgData);
                                                break;
                                            }
                                        }
                                    }
                                }
                                catch {}
                            });

                            // Return 1x1 transparent PNG immediately so Flash UI / battle loop does not stall
                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "image/png";
                            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                            context.Response.Headers.Append("Cache-Control", "no-cache");
                            await context.Response.Body.WriteAsync(transparentPixel);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        bool isCraterAsset = normalizedPath.Contains("/bomb/crater/", StringComparison.OrdinalIgnoreCase);
                        if (isCraterAsset)
                        {
                            string fallbackName = (normalizedPath.EndsWith("/craterbrink.png", StringComparison.OrdinalIgnoreCase) ||
                                                   normalizedPath.EndsWith("/craterBrink.png", StringComparison.OrdinalIgnoreCase))
                                ? "craterbrink.png" : "crater.png";
                            string fallbackPath = Path.Combine(resourcesDir, "image", "bomb", "crater", "4", fallbackName);
                            if (File.Exists(fallbackPath)
                                && DigAssetSanitizer.TryLoadSanitized(
                                    "image/bomb/crater/4/" + fallbackName, fallbackPath, out byte[] fallbackBytes2))
                            {
                                context.Response.StatusCode = 200;
                                context.Response.ContentType = "image/png";
                                context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                                context.Response.Headers.Append("Cache-Control", "no-cache, max-age=0, must-revalidate");
                                await context.Response.Body.WriteAsync(fallbackBytes2);
                                return;
                            }
                        }
                        else if (normalizedPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        {
                            // Offline or network error -> return 1x1 transparent PNG immediately so game runs offline without hanging!
                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "image/png";
                            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                            context.Response.Headers.Append("Cache-Control", "no-cache");
                            await context.Response.Body.WriteAsync(transparentPixel);
                            return;
                        }
                        Console.WriteLine($"[Asset Proxy Offline Fallback] {path}: {ex.Message}");
                    }
                }

                await next();
            });

            if (Directory.Exists(flashDir))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(flashDir),
                    RequestPath = "/flash",
                    ContentTypeProvider = contentTypeProvider
                });
            }

            if (Directory.Exists(tankFlashDir))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(tankFlashDir),
                    RequestPath = "/web",
                    ContentTypeProvider = contentTypeProvider
                });
            }

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = contentTypeProvider
            });

            if (Directory.Exists(tankRequestDir))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(tankRequestDir),
                    ContentTypeProvider = contentTypeProvider
                });
            }

            app.UseSystemWebAdapters();

            // Admin Resource Toggle Endpoints (Strictly Admin UserID = 1 only)
            app.MapGet("/api/admin/toggle-resource", async (Microsoft.AspNetCore.Http.HttpContext context) =>
            {
                string user = context.Request.Query["user"].ToString();
                if (!string.Equals(user, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { success = false, message = "Chỉ tài khoản admin mới có quyền thực hiện!" });
                    return;
                }

                string connStr = "Server=127.0.0.1,1433;Database=Project_Player34;User Id=sa;Password=Gunny@2026Mac;TrustServerCertificate=True;";
                using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Money, Grade FROM Sys_Users_Detail WHERE UserID = 1;";
                using var reader = await cmd.ExecuteReaderAsync();
                bool isMax = false;
                if (await reader.ReadAsync())
                {
                    int money = Convert.ToInt32(reader["Money"]);
                    int grade = Convert.ToInt32(reader["Grade"]);
                    if (money > 50000000 || grade >= 65)
                    {
                        isMax = true;
                    }
                }
                await reader.CloseAsync();

                bool toMax = !isMax;
                cmd.CommandText = toMax ?
                    @"UPDATE Sys_Users_Detail 
                      SET Grade = 65, GP = 150000000, Money = 99999999, Gold = 99999999, GiftToken = 99999999, Medal = 999999 
                      WHERE UserID = 1;" :
                    @"UPDATE Sys_Users_Detail 
                      SET Grade = 30, GP = 2500000, Money = 50000, Gold = 100000, GiftToken = 20000, Medal = 500 
                      WHERE UserID = 1;";
                await cmd.ExecuteNonQueryAsync();

                context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                await context.Response.WriteAsJsonAsync(new
                {
                    success = true,
                    isMax = toMax,
                    message = toMax ?
                        "⚡ Đã BẬT Max Tài Nguyên (Level 65, 99.999.999 Xu/Vàng/Lễ Kim)! Nếu đang trong game, gõ /max để cập nhật ngay." :
                        "🛡️ Đã TẮT Max Tài Nguyên (Level 30, tài nguyên chuẩn cân bằng)! Nếu đang trong game, gõ /normal để cập nhật ngay."
                });
            });

            app.MapGet("/api/admin/resource-status", async (Microsoft.AspNetCore.Http.HttpContext context) =>
            {
                string user = context.Request.Query["user"].ToString();
                if (!string.Equals(user, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { success = false, message = "Chỉ tài khoản admin mới có quyền xem!" });
                    return;
                }

                string connStr = "Server=127.0.0.1,1433;Database=Project_Player34;User Id=sa;Password=Gunny@2026Mac;TrustServerCertificate=True;";
                using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Grade, Money, Gold, GiftToken, Medal FROM Sys_Users_Detail WHERE UserID = 1;";
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int grade = Convert.ToInt32(reader["Grade"]);
                    int money = Convert.ToInt32(reader["Money"]);
                    bool isMax = money > 50000000 || grade >= 65;
                    context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = true,
                        isMax = isMax,
                        grade = grade,
                        money = money,
                        gold = Convert.ToInt32(reader["Gold"]),
                        giftToken = Convert.ToInt32(reader["GiftToken"]),
                        medal = Convert.ToInt32(reader["Medal"])
                    });
                    return;
                }
                context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                await context.Response.WriteAsJsonAsync(new { success = false, message = "User not found" });
            });

            // Enable WebSockets for Ruffle ActionScript Socket proxy
            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(60)
            });

            app.Map("/ws", async (Microsoft.AspNetCore.Http.HttpContext context) =>
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("WebSocket endpoint only.");
                    return;
                }

                int port = 9500;
                if (int.TryParse(context.Request.Query["port"], out int reqPort))
                {
                    if (reqPort == 9569 || reqPort == 9500 || reqPort == 9200 || reqPort == 9269)
                    {
                        port = 9500;
                    }
                    else
                    {
                        port = reqPort;
                    }
                }

                string subProtocol = context.WebSockets.WebSocketRequestedProtocols.FirstOrDefault();
                WebSocket webSocket;
                try
                {
                    webSocket = subProtocol != null
                        ? await context.WebSockets.AcceptWebSocketAsync(subProtocol)
                        : await context.WebSockets.AcceptWebSocketAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WS Proxy] Accept error: {ex.Message}");
                    context.Response.StatusCode = 500;
                    return;
                }

                Console.WriteLine($"[WS Proxy] Client connected from {context.Connection.RemoteIpAddress}. Forwarding to 127.0.0.1:{port}...");

                using (webSocket)
                using (var tcpClient = new TcpClient())
                {
                    try
                    {
                        await tcpClient.ConnectAsync(IPAddress.Loopback, port);
                        using var netStream = tcpClient.GetStream();
                        using var cts = new CancellationTokenSource();

                        var wsToTcp = Task.Run(async () =>
                        {
                            var buffer = new byte[32768];
                            try
                            {
                                while (webSocket.State == WebSocketState.Open && !cts.IsCancellationRequested)
                                {
                                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                                    if (result.MessageType == WebSocketMessageType.Close)
                                    {
                                        break;
                                    }
                                    if (result.Count > 0)
                                    {
                                        await netStream.WriteAsync(buffer.AsMemory(0, result.Count), cts.Token);
                                        await netStream.FlushAsync(cts.Token);
                                    }
                                }
                            }
                            catch {}
                            finally
                            {
                                cts.Cancel();
                            }
                        });

                        var tcpToWs = Task.Run(async () =>
                        {
                            var buffer = new byte[32768];
                            try
                            {
                                while (webSocket.State == WebSocketState.Open && !cts.IsCancellationRequested)
                                {
                                    int read = await netStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cts.Token);
                                    if (read == 0)
                                    {
                                        break;
                                    }
                                    await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, read), WebSocketMessageType.Binary, true, cts.Token);
                                }
                            }
                            catch {}
                            finally
                            {
                                cts.Cancel();
                            }
                        });

                        await Task.WhenAny(wsToTcp, tcpToWs);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WS Proxy] Tunnel error for port {port}: {ex.Message}");
                    }
                    finally
                    {
                        if (webSocket.State == WebSocketState.Open)
                        {
                            try
                            {
                                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                            }
                            catch {}
                        }
                        Console.WriteLine($"[WS Proxy] Disconnected from 127.0.0.1:{port}");
                    }
                }
            });

            // Initialize Gunny managers
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            LanguageMgr.Setup(baseDir);
            log4net.Config.XmlConfigurator.Configure();
            StaticsMgr.Setup();
            PlayerManager.Setup();

            // Discover all IHttpHandler types
            var asm = typeof(Program).Assembly;
            foreach (var type in asm.GetTypes().Where(t => typeof(IHttpHandler).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface))
            {
                // Prefer root Tank.Request namespace handlers over sub-namespaces
                if (type.Namespace == "Tank.Request" || !_handlers.ContainsKey(type.Name))
                {
                    _handlers[type.Name] = type;
                }
                if (!string.IsNullOrEmpty(type.FullName))
                {
                    _handlers[type.FullName] = type;
                }
            }
            // Explicitly ensure critical root handlers
            _handlers["Login"] = typeof(Tank.Request.Login);

            Console.WriteLine($"[Tank.Request] Loaded {_handlers.Count} HTTP handlers. Listening on http://0.0.0.0:828");

            // Dispatch *.ashx and *.aspx requests and missing XML fallbacks
            app.Use(async (context, next) =>
            {
                string path = context.Request.Path.Value?.TrimStart('/') ?? "";

                // Handle server_list.html
                if (path.Equals("server_list.html", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.Redirect("/web/play.html");
                    return;
                }

                // Allow crossdomain.xml
                if (path.Equals("crossdomain.xml", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.ContentType = "text/xml";
                    await context.Response.WriteAsync("<?xml version=\"1.0\"?><cross-domain-policy><allow-access-from domain=\"*\" to-ports=\"*\" /></cross-domain-policy>");
                    return;
                }

                // Check for XML file in Tank.Request directory
                if (path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    string xmlFile = Path.Combine(tankRequestDir, path);
                    if (File.Exists(xmlFile))
                    {
                        context.Response.ContentType = "text/xml";
                        await context.Response.SendFileAsync(xmlFile);
                        return;
                    }
                }

                // Check for handler
                string cleanName = Path.GetFileNameWithoutExtension(path);
                if ((path.EndsWith(".ashx", StringComparison.OrdinalIgnoreCase) || 
                     path.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase)) &&
                    _handlers.TryGetValue(cleanName, out Type handlerType))
                {
                    var handler = (IHttpHandler)Activator.CreateInstance(handlerType);
                    var sysWebContext = System.Web.HttpContext.Current;
                    if (sysWebContext != null)
                    {
                        var originalBody = context.Response.Body;
                        using var memoryStream = new MemoryStream();
                        context.Response.Body = memoryStream;

                        try
                        {
                            handler.ProcessRequest(sysWebContext);
                            try
                            {
                                sysWebContext.Response.Flush();
                            }
                            catch {}

                            memoryStream.Position = 0;
                            context.Response.Headers.ContentLength = memoryStream.Length;
                            await memoryStream.CopyToAsync(originalBody);
                            return;
                        }
                        finally
                        {
                            context.Response.Body = originalBody;
                        }
                    }
                }

                await next();
            });

            app.Run();
        }

        private static string NormalizeAssetPath(string path)
        {
            if (path.EndsWith("/craterBrink.png", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith("/craterbrink.png", StringComparison.OrdinalIgnoreCase))
            {
                int slash = path.LastIndexOf('/');
                return path.Substring(0, slash + 1) + "craterbrink.png";
            }
            return path;
        }

        private static List<string> GetAssetPathCandidates(string normalizedPath)
        {
            var list = new List<string> { normalizedPath };
            if (normalizedPath.EndsWith("/craterbrink.png", StringComparison.OrdinalIgnoreCase))
            {
                int slash = normalizedPath.LastIndexOf('/');
                string camel = normalizedPath.Substring(0, slash + 1) + "craterBrink.png";
                if (!list.Contains(camel, StringComparer.OrdinalIgnoreCase))
                {
                    list.Add(camel);
                }
            }
            if (normalizedPath.StartsWith("image/arm/", StringComparison.OrdinalIgnoreCase))
            {
                if (normalizedPath.Contains("/0/icon.png"))
                {
                    string alt = normalizedPath.Replace("/0/icon.png", "/1/icon.png");
                    if (!list.Contains(alt)) list.Add(alt);
                }
                else if (normalizedPath.Contains("/1/icon.png"))
                {
                    string alt = normalizedPath.Replace("/1/icon.png", "/0/icon.png");
                    if (!list.Contains(alt)) list.Add(alt);
                }

                if (normalizedPath.Contains("/0/0/show.png"))
                {
                    string alt = normalizedPath.Replace("/0/0/show.png", "/1/0/show.png");
                    if (!list.Contains(alt)) list.Add(alt);
                }
                else if (normalizedPath.Contains("/1/0/show.png"))
                {
                    string alt = normalizedPath.Replace("/1/0/show.png", "/0/0/show.png");
                    if (!list.Contains(alt)) list.Add(alt);
                }
            }
            return list;
        }

        private static string? ResolveLocalAssetPath(string resourcesDir, List<string> candidatePaths)
        {
            foreach (string candidate in candidatePaths)
            {
                string direct = Path.Combine(resourcesDir, candidate.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(direct))
                {
                    return direct;
                }

                // Case-insensitive lookup (Linux deploy + mixed craterBrink/craterbrink)
                string? dir = Path.GetDirectoryName(direct);
                string fileName = Path.GetFileName(direct);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    string? hit = Directory.EnumerateFiles(dir)
                        .FirstOrDefault(f => string.Equals(Path.GetFileName(f), fileName, StringComparison.OrdinalIgnoreCase));
                    if (hit != null)
                    {
                        return hit;
                    }
                }
            }
            return null;
        }
    }
}
