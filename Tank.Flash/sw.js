/* Minimal SW — giúp Chrome hiện “Cài đặt ứng dụng” / lối tắt với icon PWA.
   Không cache game assets (tránh kẹt bản SWF/cấu hình cũ). */
self.addEventListener("install", function (e) {
  self.skipWaiting();
});
self.addEventListener("activate", function (e) {
  e.waitUntil(self.clients.claim());
});
self.addEventListener("fetch", function (e) {
  e.respondWith(fetch(e.request));
});
