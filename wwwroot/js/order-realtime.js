(function () {
    'use strict';

    if (typeof signalR === 'undefined') return;

    const orderElements = Array.from(document.querySelectorAll('[data-realtime-order-id]'));
    if (orderElements.length === 0) return;

    const orderIds = [...new Set(orderElements.map(element => element.dataset.realtimeOrderId).filter(Boolean))];
    const badgeClasses = ['badge-pending', 'badge-ready', 'badge-delivered', 'badge-cancelled'];
    const statusClasses = {
        Pending: 'badge-pending',
        Ready: 'badge-ready',
        Delivered: 'badge-delivered',
        Cancelled: 'badge-cancelled'
    };

    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/order')
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .build();

    async function joinVisibleOrders() {
        for (const orderId of orderIds) {
            await connection.invoke('JoinOrderGroup', orderId);
        }
    }

    function updateOrder(orderId, status, statusText) {
        document.querySelectorAll(`[data-realtime-order-id="${CSS.escape(String(orderId))}"]`).forEach(container => {
            const badge = container.querySelector('[data-order-status]');
            if (badge) {
                badge.classList.remove(...badgeClasses);
                if (statusClasses[status]) badge.classList.add(statusClasses[status]);
                badge.dataset.orderStatus = status;
                badge.textContent = statusText;
            }

            container.dispatchEvent(new CustomEvent('salestore:order-status-changed', {
                detail: { orderId: Number(orderId), status, statusText }
            }));
        });
    }

    connection.on('OrderStatusChanged', updateOrder);
    connection.onreconnected(() => joinVisibleOrders().catch(() => { }));

    async function start() {
        try {
            await connection.start();
            await joinVisibleOrders();
        } catch {
            // Server-rendered status remains the refresh fallback during an outage.
            window.setTimeout(start, 5000);
        }
    }

    start();
})();
