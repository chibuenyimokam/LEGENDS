/* ══════════════════════════════════════════
   LegendPay — UserSupport.js
   Full functionality: SignalR, Emoji, 
   Attachment, Notifications, Search, Toast
   ══════════════════════════════════════════ */

// ── EMOJI LIST ──
const EMOJIS = [
    '😊', '😂', '🙏', '👍', '❤️', '😢', '😮', '😡',
    '🎉', '✅', '❌', '⚠️', '🔥', '💯', '🤔', '😅',
    '👋', '💪', '🙌', '😍', '🥺', '😭', '😤', '🤝',
    '📎', '📋', '📞', '💬', '🔒', '✉️', '📧', '📱',
    '💰', '💳', '🏦', '🧾', '⏰', '📅', '🔔', 'ℹ️'
];

// ── SIGNALR CONNECTION ──
let connection = null;

function initSignalR() {
    const chatIdInput = document.getElementById('chatIdInput');
    if (!chatIdInput) return;

    connection = new signalR.HubConnectionBuilder()
        .withUrl('/supportChatHub')
        .withAutomaticReconnect()
        .build();

    connection.on('ReceiveMessage', function (sender, messageText, time) {
        appendMessage(sender, messageText, time);
        scrollToBottom();
        if (sender !== 'User') {
            showToast('New message from Support Agent', 'info');
            triggerNotificationBell();
        }
    });

    connection.on('ChatStatusChanged', function (newStatus) {
        updateStatusPill(newStatus);
        showToast('Ticket status updated to: ' + (newStatus === 'InProgress' ? 'In Progress' : newStatus), 'info');
    });

    connection.start()
        .then(function () {
            const chatId = chatIdInput.value;
            if (chatId) {
                connection.invoke('JoinChat', chatId);
            }
        })
        .catch(function (err) {
            console.error('SignalR error:', err);
        });
}

// ── APPEND MESSAGE ──
function appendMessage(sender, messageText, time) {
    const container = document.getElementById('messagesContainer');
    if (!container) return;

    const isUser = sender === 'User';
    const userName = document.querySelector('.nav-avatar')?.textContent?.trim() || 'U';

    const row = document.createElement('div');
    row.className = `msg-row${isUser ? ' user-msg' : ''}`;
    row.style.maxWidth = '75%';
    if (isUser) row.style.alignSelf = 'flex-end';

    const avatar = document.createElement('div');
    avatar.className = `msg-avatar ${isUser ? 'avatar-user' : 'avatar-agent'}`;
    avatar.textContent = isUser ? userName : '';
    if (!isUser) {
        avatar.innerHTML = `<svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
            <path d="M3 18v-6a9 9 0 0 1 18 0v6"/>
            <path d="M21 19a2 2 0 0 1-2 2h-1a2 2 0 0 1-2-2v-3a2 2 0 0 1 2-2h3z"/>
            <path d="M3 19a2 2 0 0 0 2 2h1a2 2 0 0 0 2-2v-3a2 2 0 0 0-2-2H3z"/>
        </svg>`;
    }

    const content = document.createElement('div');
    content.className = 'msg-content';

    const bubble = document.createElement('div');
    bubble.className = 'msg-bubble';
    bubble.textContent = messageText;

    const timeSpan = document.createElement('span');
    timeSpan.className = 'msg-time';
    timeSpan.innerHTML = isUser
        ? `${time} <span class="read-receipt">• READ ✓✓</span>`
        : `${time} • SUPPORT AGENT`;

    content.appendChild(bubble);
    content.appendChild(timeSpan);

    if (isUser) {
        row.appendChild(content);
        row.appendChild(avatar);
    } else {
        row.appendChild(avatar);
        row.appendChild(content);
    }

    container.appendChild(row);
}

// ── SCROLL TO BOTTOM ──
function scrollToBottom() {
    const container = document.getElementById('messagesContainer');
    if (container) {
        container.scrollTop = container.scrollHeight;
    }
}

// ── UPDATE STATUS PILL ──
function updateStatusPill(newStatus) {
    const pill = document.getElementById('statusPill');
    if (!pill) return;
    const classMap = { Open: 'open', InProgress: 'inprogress', Resolved: 'resolved', Closed: 'closed' };
    pill.className = 'status-pill ' + (classMap[newStatus] || 'open');
    pill.textContent = newStatus === 'InProgress' ? 'In Progress' : newStatus;
}

// ── SEND MESSAGE ──
function submitMessage() {
    const input = document.getElementById('messageInput');
    const hidden = document.getElementById('messageTextHidden');
    const chatId = document.getElementById('chatIdInput')?.value;
    const hasFile = attachmentInput && attachmentInput.files && attachmentInput.files.length > 0;

    if (!input || !chatId) return;
    if (input.value.trim() === '' && !hasFile) return;

    hidden.value = input.value.trim();

    // Close emoji picker
    document.getElementById('emojiPicker')?.classList.remove('show');

    document.getElementById('sendMessageForm').submit();
}

document.getElementById('sendBtn')?.addEventListener('click', submitMessage);

document.getElementById('messageInput')?.addEventListener('keydown', function (e) {
    if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        submitMessage();
    }
});

// ── ATTACHMENT ──
const attachmentInput = document.getElementById('attachmentInput');
const attachmentPreviewBar = document.getElementById('attachmentPreviewBar');
const attachmentFileName = document.getElementById('attachmentFileName');
const attachmentRemoveBtn = document.getElementById('attachmentRemoveBtn');

attachmentInput?.addEventListener('change', function () {
    if (this.files && this.files.length > 0) {
        const file = this.files[0];
        const maxSize = 5 * 1024 * 1024; // 5MB
        if (file.size > maxSize) {
            showToast('File too large. Maximum size is 5MB.', 'error');
            this.value = '';
            return;
        }
        attachmentFileName.textContent = file.name;
        attachmentPreviewBar.style.display = 'block';
    }
});

attachmentRemoveBtn?.addEventListener('click', function () {
    hideAttachmentBar();
});

function hideAttachmentBar() {
    if (attachmentInput) attachmentInput.value = '';
    if (attachmentPreviewBar) attachmentPreviewBar.style.display = 'none';
    if (attachmentFileName) attachmentFileName.textContent = 'No file selected';
}

// ── EMOJI PICKER ──
const emojiBtn = document.getElementById('emojiBtn');
const emojiPicker = document.getElementById('emojiPicker');
const emojiGrid = document.getElementById('emojiGrid');

if (emojiGrid) {
    EMOJIS.forEach(function (emoji) {
        const span = document.createElement('span');
        span.className = 'emoji-item';
        span.textContent = emoji;
        span.addEventListener('click', function () {
            const input = document.getElementById('messageInput');
            if (input) {
                const pos = input.selectionStart || input.value.length;
                input.value = input.value.substring(0, pos) + emoji + input.value.substring(pos);
                input.focus();
                input.selectionStart = input.selectionEnd = pos + emoji.length;
            }
            emojiPicker.classList.remove('show');
        });
        emojiGrid.appendChild(span);
    });
}

emojiBtn?.addEventListener('click', function (e) {
    e.stopPropagation();
    emojiPicker?.classList.toggle('show');
});

document.addEventListener('click', function (e) {
    if (!e.target.closest('.emoji-wrapper')) {
        emojiPicker?.classList.remove('show');
    }
    if (!e.target.closest('.notif-wrapper')) {
        document.getElementById('notifDropdown')?.classList.remove('show');
    }
});

// ── NEW DISPUTE MODAL ──
const disputeModal = document.getElementById('disputeModal');
const subjectInput = document.getElementById('subjectInput');
const subjectCharCount = document.getElementById('subjectCharCount');

function openModal() {
    disputeModal?.classList.add('show');
    subjectInput?.focus();
}

function closeModal() {
    disputeModal?.classList.remove('show');
    if (subjectInput) subjectInput.value = '';
    if (subjectCharCount) subjectCharCount.textContent = '0';
}

document.getElementById('newDisputeBtn')?.addEventListener('click', openModal);
document.getElementById('newDisputeBtnInline')?.addEventListener('click', openModal);
document.getElementById('modalCancelBtn')?.addEventListener('click', closeModal);
document.getElementById('modalCloseBtn')?.addEventListener('click', closeModal);

disputeModal?.addEventListener('click', function (e) {
    if (e.target === disputeModal) closeModal();
});

subjectInput?.addEventListener('input', function () {
    if (subjectCharCount) subjectCharCount.textContent = this.value.length;
});

subjectInput?.addEventListener('keydown', function (e) {
    if (e.key === 'Enter') {
        e.preventDefault();
        document.getElementById('modalSubmitBtn')?.click();
    }
});

document.getElementById('modalSubmitBtn')?.addEventListener('click', function () {
    const subject = subjectInput?.value.trim();
    if (!subject) {
        showToast('Please enter a subject for your dispute.', 'error');
        subjectInput?.focus();
        return;
    }
    document.getElementById('newDisputeForm').submit();
});

// ── CONVERSATION SEARCH ──
document.getElementById('searchConvos')?.addEventListener('input', function () {
    const q = this.value.toLowerCase();
    document.querySelectorAll('.convo-item').forEach(function (item) {
        const subject = item.dataset.subject || '';
        item.closest('.convo-link').style.display = subject.includes(q) ? 'block' : 'none';
    });
});

// ── NOTIFICATIONS ──
let notifications = [];
let unreadCount = 0;

const notifBell = document.getElementById('notifBell');
const notifDot = document.getElementById('notifDot');
const notifDropdown = document.getElementById('notifDropdown');
const notifList = document.getElementById('notifList');

notifBell?.addEventListener('click', function (e) {
    e.stopPropagation();
    notifDropdown?.classList.toggle('show');
    if (notifDropdown?.classList.contains('show')) {
        markAllRead();
    }
});

document.getElementById('markAllReadBtn')?.addEventListener('click', function () {
    markAllRead();
});

function triggerNotificationBell() {
    unreadCount++;
    if (notifDot) notifDot.style.display = 'block';
    // Bell shake animation
    if (notifBell) {
        notifBell.style.animation = 'none';
        notifBell.offsetHeight; // reflow
        notifBell.style.animation = 'bell-shake 0.5s ease';
    }
}

function addNotification(message) {
    const now = new Date();
    const time = now.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
    notifications.unshift({ message, time, read: false });
    renderNotifications();
    triggerNotificationBell();
}

function renderNotifications() {
    if (!notifList) return;
    if (notifications.length === 0) {
        notifList.innerHTML = '<div class="notif-empty">No new notifications</div>';
        return;
    }
    notifList.innerHTML = notifications.slice(0, 10).map(function (n, i) {
        return `<div class="notif-item ${n.read ? '' : 'unread'}">
            <div class="notif-item-msg">${n.message}</div>
            <div class="notif-item-time">${n.time}</div>
        </div>`;
    }).join('');
}

function markAllRead() {
    notifications.forEach(function (n) { n.read = true; });
    unreadCount = 0;
    if (notifDot) notifDot.style.display = 'none';
    renderNotifications();
}

// ── TOAST ──
function showToast(message, type = 'info') {
    const container = document.getElementById('toastContainer');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.innerHTML = `
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
            ${type === 'success' ? '<path d="M20 6L9 17l-5-5"/>' :
            type === 'error' ? '<line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>' :
                '<circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>'}
        </svg>
        <span>${message}</span>`;

    container.appendChild(toast);

    setTimeout(function () {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(20px)';
        toast.style.transition = 'all 0.3s ease';
        setTimeout(function () { toast.remove(); }, 300);
    }, 3500);
}

// ── BELL SHAKE KEYFRAME (inject into page) ──
(function () {
    const style = document.createElement('style');
    style.textContent = `
        @keyframes bell-shake {
            0%, 100% { transform: rotate(0deg); }
            20%       { transform: rotate(-15deg); }
            40%       { transform: rotate(15deg); }
            60%       { transform: rotate(-10deg); }
            80%       { transform: rotate(10deg); }
        }
    `;
    document.head.appendChild(style);
})();

// ── INIT ──
window.addEventListener('DOMContentLoaded', function () {
    scrollToBottom();
    initSignalR();
    renderNotifications();
});