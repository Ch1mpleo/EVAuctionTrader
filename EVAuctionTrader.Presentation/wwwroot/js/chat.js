// Chat functionality with SignalR for Razor Pages
(function () {
    'use strict';

    let connection = null;
    let currentConversationId = null;
    let conversations = [];

    const chatToggleBtn = document.getElementById('chatToggleBtn');
    const chatPanel = document.getElementById('chatPanel');
    const chatCloseBtn = document.getElementById('chatCloseBtn');
    const chatBadge = document.getElementById('chatBadge');
    const conversationsView = document.getElementById('conversationsView');
    const conversationsList = document.getElementById('conversationsList');
    const messagesContainer = document.getElementById('messagesContainer');
    const messagesList = document.getElementById('messagesList');
    const messageInput = document.getElementById('messageInput');
    const sendBtn = document.getElementById('sendBtn');
    const backBtn = document.getElementById('backBtn');
    const conversationHeaderInfo = document.getElementById('conversationHeaderInfo');

    // ✅ Initialize SignalR connection
    async function initializeSignalR() {
        const token = getAuthToken();

        connection = new signalR.HubConnectionBuilder()
            .withUrl('/chathub', {
                accessTokenFactory: () => token,
                skipNegotiation: false,
                transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        connection.on('ReceiveMessage', onReceiveMessage);
        connection.on('ConversationUpdated', onConversationUpdated);
        connection.on('MessagesMarkedAsRead', onMessagesMarkedAsRead);

        connection.onreconnecting((error) => {
            console.warn('SignalR reconnecting:', error);
            showConnectionStatus('Reconnecting...');
        });

        connection.onreconnected((connectionId) => {
            console.log('SignalR reconnected:', connectionId);
            showConnectionStatus('Connected');
            loadConversations();
        });

        connection.onclose((error) => {
            console.error('SignalR connection closed:', error);
            showConnectionStatus('Disconnected');
            setTimeout(initializeSignalR, 5000);
        });

        try {
            await connection.start();
            console.log('✅ SignalR connected successfully');
            showConnectionStatus('Connected');
            await loadConversations();
            await updateUnreadCount();
        } catch (err) {
            console.error('❌ SignalR connection error:', err);
            showConnectionStatus('Connection failed');
            showConnectionError();
            setTimeout(initializeSignalR, 5000);
        }
    }

    // ✅ Get auth token from session
    function getAuthToken() {
        // Try sessionStorage first (for SignalR)
        let token = sessionStorage.getItem('AuthToken');

        // Fallback: try to get from a meta tag or cookie if needed
        if (!token) {
            // You might need to set this in your _Layout.cshtml
            const metaToken = document.querySelector('meta[name="auth-token"]');
            if (metaToken) {
                token = metaToken.getAttribute('content');
                sessionStorage.setItem('AuthToken', token);
            }
        }

        return token || '';
    }

    // ✅ Show connection status
    function showConnectionStatus(status) {
        console.log(`Chat status: ${status}`);
    }

    // ✅ Show connection error
    function showConnectionError() {
        if (conversationsList) {
            conversationsList.innerHTML = `
                <div class="empty-state error-state">
                    <i class="bi bi-exclamation-triangle text-warning"></i>
                    <p>Connection error. Retrying...</p>
                </div>
            `;
        }
    }

    // ✅ Expose reload function globally
    window.chatReloadConversations = loadConversations;

    // ✅ Load conversations using Razor Pages handler
    async function loadConversations() {
        try {
            const response = await fetch('/Chat/Conversations');

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            conversations = await response.json();
            renderConversations();
            updateUnreadCount();
        } catch (err) {
            console.error('Error loading conversations:', err);
            showConnectionError();
        }
    }

    // Render conversations list
    function renderConversations() {
        if (!conversationsList) return;

        if (conversations.length === 0) {
            conversationsList.innerHTML = `
                <div class="empty-state">
                    <i class="bi bi-chat-square-text"></i>
                    <p>No conversations yet</p>
                </div>
            `;
            return;
        }

        conversationsList.innerHTML = conversations.map(conv => {
            const otherUser = getCurrentUserId() === conv.buyerId ? conv.sellerName : conv.buyerName;
            const initial = otherUser.charAt(0).toUpperCase();
            const lastMessageText = conv.lastMessage?.body || 'No messages yet';
            const timeAgo = conv.lastMessage ? formatTimeAgo(new Date(conv.lastMessage.createdAt)) : '';

            return `
                <div class="conversation-item ${conv.id === currentConversationId ? 'active' : ''}" 
                     data-conversation-id="${conv.id}">
                    <div class="conversation-avatar">${initial}</div>
                    <div class="conversation-info">
                        <div class="conversation-name">
                            <span>${escapeHtml(otherUser)}</span>
                            <span class="conversation-time">${timeAgo}</span>
                        </div>
                        <div class="conversation-post-title">
                            <i class="bi bi-tag"></i> ${escapeHtml(conv.postTitle)}
                        </div>
                        <div class="conversation-preview">${escapeHtml(lastMessageText)}</div>
                    </div>
                    ${conv.unreadCount > 0 ? `<div class="conversation-unread">${conv.unreadCount}</div>` : ''}
                </div>
            `;
        }).join('');

        // Add click handlers
        document.querySelectorAll('.conversation-item').forEach(item => {
            item.addEventListener('click', () => {
                const convId = item.dataset.conversationId;
                openConversation(convId);
            });
        });
    }

    // Open conversation
    async function openConversation(conversationId) {
        currentConversationId = conversationId;
        const conversation = conversations.find(c => c.id === conversationId);

        if (!conversation) return;

        const otherUser = getCurrentUserId() === conversation.buyerId ? conversation.sellerName : conversation.buyerName;

        if (conversationHeaderInfo) {
            // ✅ Hiển thị thông tin chi tiết với hình ảnh sản phẩm
            const postImageHtml = conversation.postPhotoUrl 
                ? `<img src="${escapeHtml(conversation.postPhotoUrl)}" alt="Product" class="post-image-header" onerror="this.style.display='none';">`
                : '';

            conversationHeaderInfo.innerHTML = `
                <div class="header-user-info">
                    <h4>${escapeHtml(otherUser)}</h4>
                    <p><i class="bi bi-tag"></i> ${escapeHtml(conversation.postTitle)}</p>
                </div>
                ${postImageHtml}
            `;
        }

        if (conversationsView && messagesContainer) {
            conversationsView.style.display = 'none';
            messagesContainer.classList.add('active');
        }

        await loadMessages(conversationId);
        await markAsRead(conversationId);
    }

    // ✅ Load messages using Razor Pages handler
    async function loadMessages(conversationId) {
        try {
            const response = await fetch(`/Chat/Conversations?handler=Messages&id=${conversationId}`);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const messages = await response.json();
            renderMessages(messages);
        } catch (err) {
            console.error('Error loading messages:', err);
            if (messagesList) {
                messagesList.innerHTML = `
                    <div class="empty-state error-state">
                        <i class="bi bi-exclamation-triangle text-warning"></i>
                        <p>Error loading messages</p>
                    </div>
                `;
            }
        }
    }

    // Render messages
    function renderMessages(messages) {
        if (!messagesList) return;

        if (messages.length === 0) {
            messagesList.innerHTML = `
                <div class="empty-state">
                    <i class="bi bi-chat"></i>
                    <p>No messages yet. Start the conversation!</p>
                </div>
            `;
            return;
        }

        messagesList.innerHTML = messages.map(msg => `
            <div class="message-item ${msg.isCurrentUser ? 'sent' : 'received'}">
                <div class="message-bubble">${escapeHtml(msg.body)}</div>
                <div class="message-time">${formatTime(new Date(msg.createdAt))}</div>
            </div>
        `).join('');

        messagesList.scrollTop = messagesList.scrollHeight;
    }

    // Send message via SignalR
    async function sendMessage() {
        const body = messageInput?.value.trim();

        if (!body || !currentConversationId || !connection) return;

        try {
            await connection.invoke('SendMessage', {
                conversationId: currentConversationId,
                body: body
            });

            if (messageInput) {
                messageInput.value = '';
                messageInput.style.height = 'auto';
            }
        } catch (err) {
            console.error('Error sending message:', err);
            alert('Failed to send message. Please try again.');
        }
    }

    // Mark as read via SignalR
    async function markAsRead(conversationId) {
        if (!connection) return;

        try {
            await connection.invoke('MarkAsRead', conversationId);
        } catch (err) {
            console.error('Error marking as read:', err);
        }
    }

    // Update unread count
    function updateUnreadCount() {
        const totalUnread = conversations.reduce((sum, conv) => sum + conv.unreadCount, 0);

        if (chatBadge) {
            if (totalUnread > 0) {
                chatBadge.textContent = totalUnread > 99 ? '99+' : totalUnread;
                chatBadge.style.display = 'flex';
            } else {
                chatBadge.style.display = 'none';
            }
        }
    }

    // ✅ SignalR event handlers
    function onReceiveMessage(message) {
        if (message.conversationId === currentConversationId) {
            // ✅ FIX: Tự xác định isCurrentUser dựa trên senderId
            const currentUserId = getCurrentUserId();
            const isCurrentUserMessage = message.senderId === currentUserId;

            const messageHtml = `
                <div class="message-item ${isCurrentUserMessage ? 'sent' : 'received'}">
                    <div class="message-bubble">${escapeHtml(message.body)}</div>
                    <div class="message-time">${formatTime(new Date(message.createdAt))}</div>
                </div>
            `;

            if (messagesList) {
                const emptyState = messagesList.querySelector('.empty-state');
                if (emptyState) {
                    messagesList.innerHTML = '';
                }

                messagesList.insertAdjacentHTML('beforeend', messageHtml);
                messagesList.scrollTop = messagesList.scrollHeight;
            }

            // Mark as read nếu tin nhắn từ người khác
            if (!isCurrentUserMessage) {
                markAsRead(currentConversationId);
            }
        }
        
        // ✅ FIX NOTIFICATION: Luôn reload conversations khi có tin nhắn mới
        // Điều này đảm bảo badge và danh sách được cập nhật ngay lập tức
        loadConversations().then(() => {
            updateUnreadCount();
        });
    }

    function onConversationUpdated(conversation) {
        const index = conversations.findIndex(c => c.id === conversation.id);
        if (index !== -1) {
            conversations[index] = conversation;
        } else {
            conversations.unshift(conversation);
        }
        renderConversations();
        updateUnreadCount();
    }

    function onMessagesMarkedAsRead(conversationId) {
        const conv = conversations.find(c => c.id === conversationId);
        if (conv) {
            conv.unreadCount = 0;
            renderConversations();
            updateUnreadCount();
        }
    }

    // ✅ UI helpers
    function formatTimeAgo(date) {
        const seconds = Math.floor((new Date() - date) / 1000);

        if (seconds < 60) return 'Just now';
        if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
        if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`;
        if (seconds < 604800) return `${Math.floor(seconds / 86400)}d ago`;

        return date.toLocaleDateString();
    }

    function formatTime(date) {
        return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function getCurrentUserId() {
        try {
            const token = getAuthToken();
            if (!token) return null;

            const payload = JSON.parse(atob(token.split('.')[1]));
            return payload.sub || payload.nameid;
        } catch (err) {
            console.error('Error getting current user ID:', err);
            return null;
        }
    }

    // ✅ Event listeners
    chatToggleBtn?.addEventListener('click', () => {
        chatPanel?.classList.toggle('active');
        if (chatPanel?.classList.contains('active')) {
            loadConversations();
        }
    });

    chatCloseBtn?.addEventListener('click', () => {
        chatPanel?.classList.remove('active');
    });

    backBtn?.addEventListener('click', () => {
        currentConversationId = null;
        messagesContainer?.classList.remove('active');
        if (conversationsView) {
            conversationsView.style.display = 'block';
        }
        loadConversations();
    });

    sendBtn?.addEventListener('click', sendMessage);

    messageInput?.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });

    messageInput?.addEventListener('input', function () {
        this.style.height = 'auto';
        this.style.height = (this.scrollHeight) + 'px';
    });

    // ✅ Initialize on page load
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeSignalR);
    } else {
        initializeSignalR();
    }
})();