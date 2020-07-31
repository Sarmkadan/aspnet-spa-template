// =============================================================================
// Thin fetch wrapper with automatic CSRF token injection.
//
// Usage:
//   Place <meta name="csrf-token" content="@AntiForgery.GetAndStoreTokens(HttpContext).RequestToken">
//   in your Razor layout or index.html.
//
//   Then call Api.post('/api/orders', payload) and the X-CSRF-Token header
//   is attached automatically for all mutating requests (POST, PUT, PATCH, DELETE).
// =============================================================================

const Api = (() => {
    const MUTATING_METHODS = new Set(['POST', 'PUT', 'PATCH', 'DELETE']);

    function getCsrfToken() {
        return document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ?? '';
    }

    async function request(url, options = {}) {
        const method = (options.method || 'GET').toUpperCase();

        const headers = {
            ...(options.headers || {})
        };

        // Automatically inject the antiforgery token for state-mutating requests.
        if (MUTATING_METHODS.has(method)) {
            const token = getCsrfToken();
            if (token) {
                headers['X-CSRF-Token'] = token;
            }
        }

        // Default Content-Type for requests that carry a body.
        if (options.body !== undefined && !headers['Content-Type']) {
            headers['Content-Type'] = 'application/json';
        }

        const response = await fetch(url, { ...options, method, headers });
        return response;
    }

    return {
        /**
         * GET request — no CSRF token injected.
         */
        get(url, options = {}) {
            return request(url, { ...options, method: 'GET' });
        },

        /**
         * POST request — CSRF token injected automatically.
         * @param {string} url
         * @param {any} body  Serialised to JSON when a plain object is provided.
         * @param {RequestInit} [options]
         */
        post(url, body, options = {}) {
            return request(url, {
                ...options,
                method: 'POST',
                body: typeof body === 'string' ? body : JSON.stringify(body)
            });
        },

        /**
         * PUT request — CSRF token injected automatically.
         */
        put(url, body, options = {}) {
            return request(url, {
                ...options,
                method: 'PUT',
                body: typeof body === 'string' ? body : JSON.stringify(body)
            });
        },

        /**
         * PATCH request — CSRF token injected automatically.
         */
        patch(url, body, options = {}) {
            return request(url, {
                ...options,
                method: 'PATCH',
                body: typeof body === 'string' ? body : JSON.stringify(body)
            });
        },

        /**
         * DELETE request — CSRF token injected automatically.
         */
        delete(url, options = {}) {
            return request(url, { ...options, method: 'DELETE' });
        },

        /** Low-level access to the underlying request helper. */
        request
    };
})();
