// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

// Cache version will be updated by the build process or can be made dynamic
// For now, we'll use a timestamp-based approach that gets updated on each deploy
const CACHE_VERSION = 'v1'; // This should be updated with each deployment
const STATIC_CACHE = `spa-static-${CACHE_VERSION}`;
const API_CACHE = `spa-api-${CACHE_VERSION}`;

// We'll populate this dynamically from the asset manifest
let PRECACHE_ASSETS = ['/', '/index.html', '/offline.html', '/css/style.css', '/js/api.js', '/js/app.js', '/manifest.json'];

// Background sync queue tag
const SYNC_TAG = 'offline-queue';

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

// ── Background Sync ───────────────────────────────────────────────────────────

self.addEventListener('sync', (event) => {
    if (event.tag === SYNC_TAG) {
        event.waitUntil(replayOfflineQueue());
    }
});

/**
 * Replays all requests that were queued while the device was offline.
 * Entries are stored in IndexedDB by the client and replayed here once
 * the browser fires the Background Sync event.
 */
async function replayOfflineQueue() {
    const queue = await readOfflineQueue();
    if (!queue.length) return;

    const results = await Promise.allSettled(
        queue.map((entry) =>
            fetch(entry.url, {
                method: entry.method,
                headers: entry.headers,
                body: entry.body ?? undefined
            }).then((res) => {
                if (res.ok) removeFromOfflineQueue(entry.id);
                return res;
            })
        )
    );

    const failed = results.filter((r) => r.status === 'rejected').length;
    if (failed > 0) {
        throw new Error(`${failed} sync entries could not be replayed`);
    }
}

// ── Push Notifications ────────────────────────────────────────────────────────

self.addEventListener('push', (event) => {
    if (!event.data) return;

    let payload;
    try {
        payload = event.data.json();
    } catch {
        payload = { title: 'New notification', body: event.data.text() };
    }

    const { title, body, icon, badge, actionUrl, tag, data } = payload;

    const options = {
        body: body ?? '',
        icon: icon ?? '/icons/icon-192.png',
        badge: badge ?? '/icons/icon-192.png',
        tag: tag ?? 'default',
        data: { url: actionUrl ?? '/', ...data },
        requireInteraction: false,
        silent: false
    };

    event.waitUntil(self.registration.showNotification(title ?? 'Notification', options));
});

self.addEventListener('notificationclick', (event) => {
    event.notification.close();

    const targetUrl = event.notification.data?.url ?? '/';

    event.waitUntil(
        self.clients
            .matchAll({ type: 'window', includeUncontrolled: true })
            .then((windowClients) => {
                // Focus an existing tab if one is already open.
                const existing = windowClients.find((w) => {
                    try { return new URL(w.url).pathname === new URL(targetUrl, self.location.origin).pathname; } catch { return false; }
                });
                if (existing) return existing.focus();
                return self.clients.openWindow(targetUrl);
            })
    );
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
    if (event.data?.type === 'QUEUE_REQUEST') {
        writeOfflineQueue(event.data.entry);
        self.registration.sync?.register(SYNC_TAG).catch(() => {
            // Background Sync not supported — will retry on next page load.
        });
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
        // Try offline fallback for navigation requests.
        if (request.mode === 'navigate') {
            const offline = await caches.match('/offline.html');
            if (offline) return offline;
        }
        return new Response('Service unavailable while offline.', {
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

// ── Offline Queue (IndexedDB helpers) ─────────────────────────────────────────

const DB_NAME = 'spa-offline-queue';
const DB_VERSION = 1;
const STORE = 'requests';

function openDb() {
    return new Promise((resolve, reject) => {
        const req = indexedDB.open(DB_NAME, DB_VERSION);
        req.onupgradeneeded = (e) => {
            const db = e.target.result;
            if (!db.objectStoreNames.contains(STORE)) {
                db.createObjectStore(STORE, { keyPath: 'id', autoIncrement: true });
            }
        };
        req.onsuccess = (e) => resolve(e.target.result);
        req.onerror = (e) => reject(e.target.error);
    });
}

async function readOfflineQueue() {
    const db = await openDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction(STORE, 'readonly');
        const req = tx.objectStore(STORE).getAll();
        req.onsuccess = (e) => resolve(e.target.result);
        req.onerror = (e) => reject(e.target.error);
    });
}

async function writeOfflineQueue(entry) {
    const db = await openDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction(STORE, 'readwrite');
        const req = tx.objectStore(STORE).add(entry);
        req.onsuccess = () => resolve();
        req.onerror = (e) => reject(e.target.error);
    });
}

async function removeFromOfflineQueue(id) {
    const db = await openDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction(STORE, 'readwrite');
        const req = tx.objectStore(STORE).delete(id);
        req.onsuccess = () => resolve();
        req.onerror = (e) => reject(e.target.error);
    });
}

