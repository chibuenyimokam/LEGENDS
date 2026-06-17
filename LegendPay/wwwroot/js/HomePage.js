/**
 * LegendPay UI Initialization Engine
 */
document.addEventListener('DOMContentLoaded', () => {

    // ── SIDEBAR NAVIGATION HIGHLIGHTS ──
    document.querySelectorAll('.nav-item').forEach(item => {
        item.addEventListener('click', e => {
            // Only suppress default if running placeholder '#' links
            if (item.getAttribute('href') === '#') {
                e.preventDefault();
                document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));
                item.classList.add('active');
            }
        });
    });

    // ── CLIPBOARD ENGINE FOR WALLET ID ──
    const copyBtn = document.getElementById('copy-wid');
    const widText = document.getElementById('wid-text');

    if (copyBtn && widText) {
        document.getElementById('wallet-id-pill')?.addEventListener('click', () => copyBtn.click());

        copyBtn.addEventListener('click', e => {
            e.stopPropagation();
            const val = widText.textContent.trim();
            if (!val || val === '—') return;

            const handleSuccess = () => {
                const legacyMarkup = copyBtn.innerHTML;
                copyBtn.innerHTML = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                    <polyline points="20 6 9 17 4 12"/></svg>`;
                copyBtn.style.color = '#4ADE80';
                setTimeout(() => {
                    copyBtn.innerHTML = legacyMarkup;
                    copyBtn.style.color = '';
                }, 1800);
            };

            if (navigator.clipboard && window.isSecureContext) {
                navigator.clipboard.writeText(val).then(handleSuccess).catch(() => fallbackCopy(widText, handleSuccess));
            } else {
                fallbackCopy(widText, handleSuccess);
            }
        });
    }

    function fallbackCopy(textElement, callback) {
        const range = document.createRange();
        range.selectNode(textElement);
        window.getSelection().removeAllRanges();
        window.getSelection().addRange(range);
        try {
            document.execCommand('copy');
            callback();
        } catch (err) {
            console.error('Fallback copy operation failed', err);
        }
        window.getSelection().removeAllRanges();
    }

    // ── INTERACTIVE BALANCE ODOMETER ANIMATION ──
    const balanceEl = document.getElementById('balance-animated');
    if (balanceEl) {
        const rawText = balanceEl.textContent.replace(/[₦,\s]/g, '');
        const targetValue = parseFloat(rawText);
        if (!isNaN(targetValue) && targetValue > 0) {
            const initialStartValue = targetValue * 0.82;
            const animationDuration = 850;
            let timelineStart = null;

            const currencyFormatter = value => '₦' + value.toLocaleString('en-NG', {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });

            const renderFrame = timestamp => {
                if (!timelineStart) timelineStart = timestamp;
                const progressTime = Math.min((timestamp - timelineStart) / animationDuration, 1);
                const cubicEasing = 1 - Math.pow(1 - progressTime, 3); // Ease-Out Cubic

                balanceEl.textContent = currencyFormatter(initialStartValue + (targetValue - initialStartValue) * cubicEasing);

                if (progressTime < 1) {
                    requestAnimationFrame(renderFrame);
                }
            };
            requestAnimationFrame(renderFrame);
        }
    }

    // ── PROGRESS TRACKER FILL ANIMATION ──
    const progressBarFill = document.querySelector('.points-fill');
    if (progressBarFill) {
        const specifiedWidth = progressBarFill.getAttribute('data-progress') || '0%';
        requestAnimationFrame(() => {
            setTimeout(() => {
                progressBarFill.style.width = specifiedWidth;
            }, 100);
        });
    }

    // ── MVC INTERACTION AND ROUTING MAP ──
    const bindRouteAction = (elementId, targetUrl) => {
        document.getElementById(elementId)?.addEventListener('click', () => {
            window.location.href = targetUrl;
        });
    };

    bindRouteAction('btn-fund', '/Transactions/FundWallet');
    bindRouteAction('btn-topup', '/Transactions/FundWallet');
    bindRouteAction('qa-fund', '/Transactions/FundWallet');
    bindRouteAction('btn-paybill', '/Transactions/PayBill');
    bindRouteAction('qa-paybill', '/Transactions/PayBill');
    bindRouteAction('qa-airtime', '/Transactions/BuyAirtime');
    bindRouteAction('qa-rewards', '/Rewards');
});