// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

const API_BASE = '/api/v1';
const AUTH_TOKEN_KEY = 'authToken';

// Application State
const AppState = {
    user: null,
    cart: [],
    products: [],
    currentProduct: null,
    currentPage: 'home',

    init() {
        this.loadFromLocalStorage();
        this.checkAuthentication();
    },

    loadFromLocalStorage() {
        try {
            const token = localStorage.getItem(AUTH_TOKEN_KEY);
            const cart = localStorage.getItem('cart');
            if (token) this.user = { token };
            if (cart) this.cart = JSON.parse(cart);
        } catch (e) {
            console.error('Failed to load state from localStorage:', e);
        }
    },

    saveToLocalStorage() {
        if (this.user?.token) {
            localStorage.setItem(AUTH_TOKEN_KEY, this.user.token);
        }
        if (this.cart.length > 0) {
            localStorage.setItem('cart', JSON.stringify(this.cart));
        }
    },

    checkAuthentication() {
        const token = localStorage.getItem(AUTH_TOKEN_KEY);
        if (token) {
            this.user = { token };
            updateAuthUI();
        }
    }
};

// API Communication
const API = {
    async request(endpoint, options = {}) {
        const url = `${API_BASE}${endpoint}`;
        const headers = {
            'Content-Type': 'application/json',
            ...options.headers
        };

        if (AppState.user?.token) {
            headers['Authorization'] = `Bearer ${AppState.user.token}`;
        }

        try {
            const response = await fetch(url, {
                ...options,
                headers
            });

            if (response.status === 401) {
                logout();
                return null;
            }

            const data = await response.json();

            if (!response.ok) {
                showError(data.message || 'An error occurred');
                return null;
            }

            return data.data || data;
        } catch (error) {
            console.error('API request failed:', error);
            showError('Network error occurred');
            return null;
        }
    },

    get(endpoint) {
        return this.request(endpoint, { method: 'GET' });
    },

    post(endpoint, body) {
        return this.request(endpoint, {
            method: 'POST',
            body: JSON.stringify(body)
        });
    },

    put(endpoint, body) {
        return this.request(endpoint, {
            method: 'PUT',
            body: JSON.stringify(body)
        });
    },

    delete(endpoint) {
        return this.request(endpoint, { method: 'DELETE' });
    }
};

// Navigation
function navigateTo(page) {
    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    const targetPage = document.getElementById(page);
    if (targetPage) {
        targetPage.classList.add('active');
        AppState.currentPage = page;

        switch (page) {
            case 'home':
                loadFeaturedProducts();
                break;
            case 'products':
                loadProducts();
                break;
            case 'cart':
                renderCart();
                break;
            case 'profile':
                loadProfile();
                break;
        }
    }
}

// Products
async function loadFeaturedProducts() {
    const products = await API.get('/products/featured?limit=6');
    if (!products) return;

    const container = document.getElementById('featuredList');
    container.innerHTML = products.map(p => createProductCard(p)).join('');
}

async function loadProducts(pageNum = 1) {
    const products = await API.get(`/products?pageNumber=${pageNum}&pageSize=12`);
    if (!products) return;

    AppState.products = products.products;
    const container = document.getElementById('productsList');
    container.innerHTML = products.products.map(p => createProductCard(p)).join('');
}

function createProductCard(product) {
    return `
        <div class="product-card" onclick="viewProduct(${product.id})">
            <div class="product-image">${product.category}</div>
            <div class="product-info">
                <div class="product-name">${product.name}</div>
                <div class="product-price">$${product.price.toFixed(2)}</div>
                <div class="product-category">${product.category}</div>
                <div class="product-rating">★ ${product.rating.toFixed(1)}</div>
                <div class="product-actions">
                    <button class="btn btn-primary" onclick="event.stopPropagation(); addToCart(${product.id})">Add to Cart</button>
                </div>
            </div>
        </div>
    `;
}

async function viewProduct(id) {
    const product = await API.get(`/products/${id}`);
    if (!product) return;

    AppState.currentProduct = product;
    navigateTo('productDetail');

    const content = document.getElementById('productDetailContent');
    content.innerHTML = `
        <div class="product-detail">
            <div class="product-detail-image">Product Image</div>
            <div class="product-detail-info">
                <h2>${product.name}</h2>
                <div class="detail-meta">
                    <div><strong>Price:</strong> $${product.price.toFixed(2)}</div>
                    <div><strong>Stock:</strong> ${product.stockQuantity}</div>
                </div>
                <div class="detail-meta">
                    <div><strong>Category:</strong> ${product.category}</div>
                    <div><strong>Rating:</strong> ★ ${product.rating.toFixed(1)}</div>
                </div>
                <p class="product-description">${product.description}</p>
                <div class="quantity-selector">
                    <label>Quantity:</label>
                    <button class="btn btn-secondary" onclick="changeQuantity(-1)">-</button>
                    <input type="number" id="quantityInput" value="1" min="1" max="${product.stockQuantity}">
                    <button class="btn btn-secondary" onclick="changeQuantity(1)">+</button>
                </div>
                <button class="btn btn-primary" onclick="addProductToCart()">Add to Cart</button>
            </div>
        </div>
    `;
}

function changeQuantity(delta) {
    const input = document.getElementById('quantityInput');
    const newVal = Math.max(1, parseInt(input.value) + delta);
    input.value = Math.min(newVal, AppState.currentProduct.stockQuantity);
}

function addProductToCart() {
    const quantity = parseInt(document.getElementById('quantityInput').value);
    addToCart(AppState.currentProduct.id, quantity);
}

function addToCart(productId, quantity = 1) {
    const product = AppState.products.find(p => p.id === productId) || AppState.currentProduct;
    if (!product) return;

    const cartItem = AppState.cart.find(item => item.id === productId);
    if (cartItem) {
        cartItem.quantity += quantity;
    } else {
        AppState.cart.push({ ...product, quantity });
    }

    AppState.saveToLocalStorage();
    showSuccess(`${product.name} added to cart`);
}

function renderCart() {
    const container = document.getElementById('cartContent');

    if (AppState.cart.length === 0) {
        container.innerHTML = '<p class="text-muted">Your cart is empty</p>';
        return;
    }

    let subtotal = 0;
    const items = AppState.cart.map(item => {
        const itemTotal = item.price * item.quantity;
        subtotal += itemTotal;
        return `
            <div class="cart-item">
                <div>
                    <h4>${item.name}</h4>
                    <p class="text-muted">$${item.price.toFixed(2)} x ${item.quantity}</p>
                </div>
                <div>
                    <p><strong>$${itemTotal.toFixed(2)}</strong></p>
                    <button class="btn btn-danger" onclick="removeFromCart(${item.id})">Remove</button>
                </div>
            </div>
        `;
    }).join('');

    const tax = subtotal * 0.1;
    const total = subtotal + tax;

    container.innerHTML = `
        <div>${items}</div>
        <div class="cart-summary">
            <div class="cart-summary-row">
                <span>Subtotal:</span>
                <span>$${subtotal.toFixed(2)}</span>
            </div>
            <div class="cart-summary-row">
                <span>Tax (10%):</span>
                <span>$${tax.toFixed(2)}</span>
            </div>
            <div class="cart-summary-row cart-summary-total">
                <span>Total:</span>
                <span>$${total.toFixed(2)}</span>
            </div>
            <button class="btn btn-primary" style="width: 100%; margin-top: 1rem;" onclick="checkout()">
                Checkout
            </button>
        </div>
    `;
}

function removeFromCart(productId) {
    AppState.cart = AppState.cart.filter(item => item.id !== productId);
    AppState.saveToLocalStorage();
    renderCart();
}

// Authentication
async function handleLogin(event) {
    event.preventDefault();

    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;

    const response = await API.post('/users/login', { email, password });
    if (!response) return;

    AppState.user = response;
    localStorage.setItem(AUTH_TOKEN_KEY, response.token);
    updateAuthUI();
    showSuccess('Logged in successfully');
    navigateTo('home');
}

async function handleRegister(event) {
    event.preventDefault();

    const userData = {
        firstName: document.getElementById('firstName').value,
        lastName: document.getElementById('lastName').value,
        email: document.getElementById('registerEmail').value,
        password: document.getElementById('registerPassword').value,
        phoneNumber: document.getElementById('phone').value || null,
        address: document.getElementById('address').value || null,
        city: document.getElementById('city').value || null,
        postalCode: document.getElementById('postalCode').value || null,
        country: document.getElementById('country').value || null
    };

    const response = await API.post('/users/register', userData);
    if (!response) return;

    showSuccess('Account created successfully. Please log in.');
    navigateTo('login');
    event.target.reset();
}

function logout() {
    AppState.user = null;
    AppState.cart = [];
    localStorage.removeItem(AUTH_TOKEN_KEY);
    localStorage.removeItem('cart');
    updateAuthUI();
    showSuccess('Logged out successfully');
    navigateTo('home');
}

function updateAuthUI() {
    const loginLink = document.getElementById('loginLink');
    const registerLink = document.getElementById('registerLink');
    const logoutLink = document.getElementById('logoutLink');

    if (AppState.user) {
        loginLink.style.display = 'none';
        registerLink.style.display = 'none';
        logoutLink.style.display = 'inline-block';
    } else {
        loginLink.style.display = 'inline-block';
        registerLink.style.display = 'inline-block';
        logoutLink.style.display = 'none';
    }
}

// Search & Filter
async function searchProducts() {
    const searchTerm = document.getElementById('searchInput').value;
    if (!searchTerm) {
        loadProducts();
        return;
    }

    const results = await API.get(`/products/search?searchTerm=${encodeURIComponent(searchTerm)}`);
    if (!results) return;

    AppState.products = results;
    const container = document.getElementById('productsList');
    container.innerHTML = results.map(p => createProductCard(p)).join('');
}

async function filterByCategory() {
    const category = document.getElementById('categoryFilter').value;
    if (!category) {
        loadProducts();
        return;
    }

    const products = await API.get(`/products/category/${category}`);
    if (!products) return;

    AppState.products = products.products;
    const container = document.getElementById('productsList');
    container.innerHTML = products.products.map(p => createProductCard(p)).join('');
}

// Profile
async function loadProfile() {
    const profile = await API.get('/users/profile');
    if (!profile) return;

    const content = document.getElementById('profileContent');
    content.innerHTML = `
        <div class="form-container">
            <h3>${profile.firstName} ${profile.lastName}</h3>
            <p><strong>Email:</strong> ${profile.email}</p>
            <p><strong>Member since:</strong> ${new Date(profile.createdAt).toLocaleDateString()}</p>
            <button class="btn btn-secondary" onclick="navigateTo('home')">Back to Home</button>
        </div>
    `;
}

// Checkout
async function checkout() {
    if (!AppState.user) {
        showError('Please log in to checkout');
        navigateTo('login');
        return;
    }

    const orderItems = AppState.cart.map(item => ({
        productId: item.id,
        quantity: item.quantity
    }));

    const order = await API.post('/orders', {
        items: orderItems,
        shippingAddress: 'Default Address',
        notes: 'Standard delivery'
    });

    if (order) {
        AppState.cart = [];
        AppState.saveToLocalStorage();
        showSuccess('Order placed successfully!');
        renderCart();
    }
}

// UI Helpers
function showSuccess(message) {
    showNotification(message, 'alert-success');
}

function showError(message) {
    showNotification(message, 'alert-error');
}

function showNotification(message, type) {
    const notification = document.createElement('div');
    notification.className = `alert ${type}`;
    notification.textContent = message;
    document.body.insertBefore(notification, document.querySelector('main'));

    setTimeout(() => notification.remove(), 5000);
}

// ── Service Worker & Offline Support ──────────────────────────────────────────

const SW = {
    registration: null,

    async register() {
        if (!('serviceWorker' in navigator)) return;
        try {
            this.registration = await navigator.serviceWorker.register('/sw.js', { scope: '/' });
            this.registration.addEventListener('updatefound', () => {
                const worker = this.registration.installing;
                worker.addEventListener('statechange', () => {
                    if (worker.state === 'installed' && navigator.serviceWorker.controller) {
                        showSuccess('App updated — refresh for the latest version.');
                    }
                });
            });
        } catch (err) {
            console.warn('[SW] Registration failed:', err);
        }
    },

    evict(path) {
        navigator.serviceWorker?.controller?.postMessage({ type: 'ASSET_UPDATED', path });
    }
};

function initOfflineBanner() {
    const banner = document.getElementById('offlineBanner');
    if (!banner) return;
    const sync = () => { banner.style.display = navigator.onLine ? 'none' : 'block'; };
    window.addEventListener('online', sync);
    window.addEventListener('offline', sync);
    sync();
}

function initHmr() {
    const isLocal = ['localhost', '127.0.0.1', '[::1]'].includes(location.hostname);
    if (!isLocal) return;

    const es = new EventSource('/__hmr');

    es.addEventListener('asset-changed', (e) => {
        const { path } = JSON.parse(e.data);
        SW.evict(path);
        if (/\.(html|js|css)$/.test(path)) {
            window.location.reload();
        }
    });

    es.onerror = () => es.close();
}

// Initialize App
document.addEventListener('DOMContentLoaded', () => {
    AppState.init();
    updateAuthUI();
    navigateTo('home');
    SW.register();
    initOfflineBanner();
    initHmr();
});
