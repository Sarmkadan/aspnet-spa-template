// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

const CACHE_VERSION = 'v1';
const STATIC_CACHE = `spa-static-${CACHE_VERSION}`;
const API_CACHE = `spa-api-${CACHE_VERSION}`;

const PRECACHE_ASSETS = ['/', '/index.html', '/css/style.css', '/js/app.js'];

// ── Install ──────────────────────────────────────────────────────────────────

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(STATIC_CACHE)
            .then((cache) => cache.addAll(PRECACHE_ASSETS))
            .then(() => self.skipWaiting())
    );
});

// ── Activate ─────────────────────────────────────────────────────────────────

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys()
            .then((keys) =>
                Promise.all(
                    keys
                        .filter((k) => k !== STATIC_CACHE && k !== API_CACHE)
                        .map((k) => caches.delete(k))
                )
            )
            .then(() => self.clients.claim())
    );
});

// ── Fetch ─────────────────────────────────────────────────────────────────────

self.addEventListener('fetch', (event) => {
    const { request } = event;
    if (request.method !== 'GET') return;

    const url = new URL(request.url);
    if (url.origin !== self.location.origin) return;

    // Let HMR and manifest requests bypass the cache entirely.
    if (url.pathname === '/__hmr' || url.pathname === '/__asset-manifest.json') return;

    if (url.pathname.startsWith('/api/')) {
        event.respondWith(networkFirst(request, API_CACHE));
    } else {
        event.respondWith(cacheFirst(request, STATIC_CACHE));
    }
});

// ── Messages from main thread ─────────────────────────────────────────────────

self.addEventListener('message', (event) => {
    if (event.data?.type === 'ASSET_UPDATED') {
        // Evict the stale entry so the next request fetches fresh content.
        caches.open(STATIC_CACHE).then((cache) => cache.delete(event.data.path));
    }
    if (event.data?.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});

// ── Strategies ────────────────────────────────────────────────────────────────

async function cacheFirst(request, cacheName) {
    const cached = await caches.match(request);
    if (cached) return cached;

    try {
        const response = await fetch(request);
        if (response.ok) {
            const cache = await caches.open(cacheName);
            cache.put(request, response.clone());
        }
        return response;
    } catch {
        const fallback = await caches.match('/index.html');
        return fallback ?? new Response('Service unavailable while offline.', {
            status: 503,
            headers: { 'Content-Type': 'text/plain' }
        });
    }
}

async function networkFirst(request, cacheName) {
    try {
        const response = await fetch(request);
        if (response.ok) {
            const cache = await caches.open(cacheName);
            cache.put(request, response.clone());
        }
        return response;
    } catch {
        const cached = await caches.match(request);
        return cached ?? new Response(
            JSON.stringify({ error: 'Offline', message: 'Request cannot be served while offline.' }),
            { status: 503, headers: { 'Content-Type': 'application/json' } }
        );
    }
}
