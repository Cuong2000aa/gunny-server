# Cloudflare Tunnel cho Gunny

Chi can expose **HTTP/WSS :828** (Tank.Request). Road/Fighting/Center/SQL giu local.

## A) Quick Tunnel (URL tam, khong can domain)

1. Chay stack: `..\scripts\start-gunny-stack.ps1`
2. Chay: `.\start-quick-tunnel.ps1`
3. Copy URL `https://xxxx.trycloudflare.com` trong log
4. Mo: `https://xxxx.trycloudflare.com/web/play.html`

URL doi moi lan restart.

## B) Named Tunnel (domain co dinh)

Can domain dang ky DNS tren Cloudflare.

```powershell
.\setup-named-tunnel.ps1 -Hostname gunny.yourdomain.com
.\start-named-tunnel.ps1
```

Play: `https://gunny.yourdomain.com/web/play.html`

## Code da chuan bi

- `play.html`: origin/WSS dung cho HTTPS Cloudflare (khong gan `:828` tren 443)
- `Tank.Request`: `X-Forwarded-Proto/Host` + rewrite `config.xml` theo public origin

## Luu y

- Khong mo SQL `:1433` qua tunnel
- Cloudflare free WebSocket idle ~100s — van choi duoc vi game co traffic
- Can file `map/<id>/fore.map` + `dead.map` de Road/Fighting start day du
