// wwwroot/js/Home/PayBills.js

// ── Sidebar collapse (reuse same logic as UserDashboard.js) ──
const sidebar = document.getElementById('sidebar');
const mainContent = document.getElementById('main-content');
const toggleBtn = document.getElementById('sidebar-toggle');

toggleBtn.addEventListener('click', () => {
    sidebar.classList.toggle('collapsed');
    mainContent.classList.toggle('sidebar-collapsed');
});

// ── Biller search filter ──
const searchInput = document.querySelector('.pb-search');
const catCards = document.querySelectorAll('.cat-card');
const favCards = document.querySelectorAll('.fav-card');

if (searchInput) {
    searchInput.addEventListener('input', () => {
        const q = searchInput.value.toLowerCase().trim();

        catCards.forEach(card => {
            const label = card.querySelector('.cat-label')?.textContent.toLowerCase() ?? '';
            card.style.opacity = (!q || label.includes(q)) ? '1' : '0.3';
        });

        favCards.forEach(card => {
            const name = card.querySelector('.fav-name')?.textContent.toLowerCase() ?? '';
            card.style.opacity = (!q || name.includes(q)) ? '1' : '0.3';
        });
    });
}

// ── Category click (wire to payment form in a future sprint) ──
catCards.forEach(card => {
    card.addEventListener('click', () => {
        const category = card.dataset.category;
        // TODO: window.location.href = `/Home/PayBillForm?category=${encodeURIComponent(category)}`;
        console.log('Category selected:', category);
    });
});