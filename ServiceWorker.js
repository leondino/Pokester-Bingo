const cacheName = "Niffo's Games-Pokester-Bingo-0.9.1";
const contentToCache = [
    "Build/63ef2af880b23ec35c46ed7921e8408d.loader.js",
    "Build/494257f3f3ec970c6ac35132436488dc.framework.js.unityweb",
    "Build/12553a2b559b168f4b2b3bb507f3537b.data.unityweb",
    "Build/45b1ac34d67f61fbeefb8b63172957a4.wasm.unityweb",
    "TemplateData/style.css"

];

self.addEventListener('install', function (e) {
    console.log('[Service Worker] Install');
    
    e.waitUntil((async function () {
      const cache = await caches.open(cacheName);
      console.log('[Service Worker] Caching all: app shell and content');
      await cache.addAll(contentToCache);
    })());
});

// add
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.filter(name => name !== cacheName)
                    .map(name => caches.delete(name))
            );
        })
    );
});

//change
self.addEventListener('fetch', function (e) {
    e.respondWith((async function () {
        if (e.request.method !== 'GET') {
            return fetch(e.request);
        }

        try {
            const cachedResponse = await caches.match(e.request);
            if (cachedResponse) {
                console.log(`[Service Worker] Return cached: ${e.request.url}`);
                return cachedResponse;
            }

            const fetchResponse = await fetch(e.request);

            if (fetchResponse.status === 200 && isCacheable(fetchResponse)) {
                const cache = await caches.open(cacheName);
                console.log(`[Service Worker] Caching new: ${e.request.url}`);
                cache.put(e.request, fetchResponse.clone());
            }

            return fetchResponse;
        } catch (error) {
            console.log('[Service Worker] Fetch failed; returning offline page');
            return caches.match('offline.html');
        }
    })());
});

//add
function isCacheable(response) {
    const contentType = response.headers.get('content-type') || '';
    return contentType.includes('application/javascript') ||
        contentType.includes('text/css') ||
        contentType.includes('application/wasm') ||
        contentType.includes('application/octet-stream');
}
