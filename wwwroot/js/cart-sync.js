/**
 * cart-sync.js — Đồng bộ giỏ hàng giữa localStorage và Supabase (qua API server)
 * Khi đã đăng nhập: mỗi thay đổi giỏ hàng sẽ tự động lưu lên server.
 * Khi chưa đăng nhập: chỉ dùng localStorage (như cũ).
 */
(function () {
    const CART_KEY = 'ss_cart_v1';

    // isAuthenticated được set từ server-side trong mỗi page
    const isAuth = typeof window.__isAuthenticated !== 'undefined' && window.__isAuthenticated === true;

    // ── localStorage helpers ──
    function getCartLocal() {
        try { return JSON.parse(localStorage.getItem(CART_KEY)) || []; }
        catch { return []; }
    }

    function saveCartLocal(cart) {
        localStorage.setItem(CART_KEY, JSON.stringify(cart));
    }

    function clearCartLocal() {
        localStorage.removeItem(CART_KEY);
    }

    // ── Server API helpers ──
    async function fetchCartFromServer() {
        try {
            const res = await fetch('/api/cart', { credentials: 'same-origin' });
            if (res.ok) return await res.json();
        } catch (e) { console.warn('Cart sync fetch error:', e); }
        return null;
    }

    async function saveCartToServer(cart) {
        try {
            await fetch('/api/cart', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'same-origin',
                body: JSON.stringify(cart)
            });
        } catch (e) { console.warn('Cart sync save error:', e); }
    }

    async function clearCartOnServer() {
        try {
            await fetch('/api/cart', {
                method: 'DELETE',
                credentials: 'same-origin'
            });
        } catch (e) { console.warn('Cart sync clear error:', e); }
    }

    // ── Public API (gắn vào window) ──
    window.CartSync = {
        CART_KEY: CART_KEY,

        getCart: function () {
            return getCartLocal();
        },

        saveCart: function (cart) {
            saveCartLocal(cart);
            if (isAuth) {
                saveCartToServer(cart); // fire-and-forget
            }
        },

        clearCart: function () {
            clearCartLocal();
            if (isAuth) {
                clearCartOnServer();
            }
        },

        /**
         * Gọi khi trang load: nếu đã đăng nhập thì tải giỏ hàng từ server về localStorage.
         */
        loadFromServer: async function () {
            if (!isAuth) return;
            const serverCart = await fetchCartFromServer();
            if (serverCart && serverCart.length > 0) {
                saveCartLocal(serverCart);
            }
            // Nếu server trống nhưng localStorage có dữ liệu (vừa đăng nhập lần đầu trên máy này)
            // thì đẩy localStorage lên server
            else {
                const localCart = getCartLocal();
                if (localCart.length > 0) {
                    await saveCartToServer(localCart);
                }
            }
        },

        /**
         * Gọi trước khi logout: xóa localStorage.
         */
        onBeforeLogout: function () {
            clearCartLocal();
        }
    };
})();
