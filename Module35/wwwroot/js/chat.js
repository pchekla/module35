// Функции для работы с чатом
$(document).ready(function () {
    // Переменные состояния
    let isTyping = false;
    let typingTimer;
    let lastTypingNotification = 0;
    let isUserTyping = false;
    let currentUserId = ''; // Получим позже
    let friendId = ''; // Получим при инициализации

    // SignalR Connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub") // Убедитесь, что URL совпадает с тем, что в Startup.cs
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // Глобальная функция инициализации чата
    window.initChat = async function(fId, cUserId) {
        friendId = fId;
        currentUserId = cUserId; // Получаем ID текущего пользователя
        
        // Настраиваем обработчики событий SignalR
        setupSignalREventHandlers();

        // Пытаемся запустить соединение
        try {
            await connection.start();
            console.log("SignalR Connected.");
            // После успешного старта можно запросить историю сообщений (если не загружена изначально)
            // loadInitialMessages(friendId); 
        } catch (err) {
            console.error("SignalR Connection Error: ", err);
            setTimeout(window.initChat, 5000); // Попробовать переподключиться через 5 секунд
        }

        // Начальная инициализация UI
        scrollToBottom();
        initializeTypingIndicator(friendId);
    };

    // Настройка обработчиков событий SignalR
    function setupSignalREventHandlers() {
        // Получение нового сообщения
        connection.on("ReceiveMessage", (senderId, senderName, senderImage, messageText, sentDate) => {
            console.log("SignalR: ReceiveMessage triggered", { senderId, senderName, messageText, sentDate });
            const isFromCurrentUser = (senderId === currentUserId);
            
            // Вызываем appendMessage с нужными параметрами
            appendMessage(messageText, new Date(sentDate), senderName, isFromCurrentUser);
            
            scrollToBottom();
            
            // Проигрываем звук, только если сообщение не от текущего пользователя
            if (!isFromCurrentUser) {
                playMessageSound();
            }
        });

        // Получение уведомления о наборе текста
        connection.on("ReceiveTypingNotification", (senderId, isTypingNow) => {
            // Убедимся, что уведомление от друга, с которым открыт чат
            if (senderId === friendId) {
                 if (isTypingNow) {
                     $('#typing-indicator').show();
                 } else {
                     $('#typing-indicator').hide();
                 }
                 // Можно добавить/убрать CSS класс вместо show/hide для анимации
            }
        });

        // Обработка закрытия соединения
        connection.onclose(async () => {
            console.log("SignalR Disconnected.");
            // Попытка переподключения
            await window.initChat(friendId, currentUserId);
        });
    }

    // Добавляет новое сообщение в DOM
    function appendMessage(messageText, sentDate, senderName, isFromCurrentUser) {
        console.log("appendMessage called", { messageText, sentDate, senderName, isFromCurrentUser });
        const messageClass = isFromCurrentUser ? 'message-sent' : 'message-received';
        const textAlign = isFromCurrentUser ? 'text-end' : '';
        
        const formattedTime = sentDate.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' }) + ', ' + 
                              sentDate.toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' });

        const escapedText = escapeHtml(messageText);

        // Создаем элемент сообщения и добавляем его на страницу с уникальным id
        var domMessageId = 'msg-' + Date.now() + '-' + Math.floor(Math.random() * 1000);
        var messageElement = $(
            '<div class="message ' + messageClass + '" id="' + domMessageId + '">' +
                '<div class="message-bubble">' +
                    '<div class="message-content">' + escapedText.replace(/\n/g, '<br>') + '</div>' +
                    '<div class="message-time small">' + formattedTime + '</div>' +
                '</div>' +
            '</div>'
        );
        
        try {
             $("#messagesContainer").append(messageElement);
             console.log("Message element appended to #messagesContainer");
             // Анимация появления нового сообщения
             messageElement.hide().fadeIn(300);
        } catch (e) {
             console.error("Error appending message to DOM:", e);
        }
        
        // Обновляем счетчик сообщений
        updateMessageCount();
        
        // Прокручиваем чат вниз для отображения нового сообщения
        // scrollToBottom(); // Эта функция вызывается в ReceiveMessage
        
        // Снимаем выделение текста
        clearSelection();
        
        // Применяем защиту от выделения к новому сообщению
        disableTextSelection();
        
        // Дополнительная защита - повторный вызов через таймаут
        setTimeout(function() {
            clearSelection();
            disableTextSelection();
            $('<div tabindex="-1">').focus().remove();
        }, 300);
        
        console.log("appendMessage finished for:", messageText);
        return messageElement;
    }
    
    // Функция обновления счетчика сообщений
    function updateMessageCount() {
         const count = $('#messagesContainer .message').length;
         $('#message-count').text(count + ' сообщений');
    }

    // Функция экранирования HTML
    function escapeHtml(unsafe) {
        if (!unsafe) return '';
        return unsafe
             .replace(/&/g, "&amp;")
             .replace(/</g, "&lt;")
             .replace(/>/g, "&gt;")
             .replace(/"/g, "&quot;")
             .replace(/'/g, "&#039;");
     }

    // Функция прокрутки чата вниз
    function scrollToBottom() {
        const messagesContainer = document.getElementById('messagesContainer');
        if (messagesContainer) {
            // Небольшая задержка, чтобы DOM успел обновиться
            setTimeout(() => {
                 messagesContainer.scrollTop = messagesContainer.scrollHeight;
            }, 50);
        }
    }

    // Функция обновления чата (больше не нужна для получения сообщений)
    /* function refreshChat(friendId) { ... } */

    // Функция запуска таймера обновления чата (больше не нужна)
    /* function startChatRefresh(friendId) { ... } */

    // Обработка отправки формы через AJAX POST
    $('#message-form').submit(function (e) {
        e.preventDefault();

        const messageText = $('#messageText').val().trim();
        const formData = $(this).serialize(); // Получаем данные формы (включая friendId и Text)

        if (!messageText || !friendId) return;

        $.ajax({
            url: '/Messages/SendMessage', // URL эндпоинта контроллера
            type: 'POST',
            data: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() // Передаем токен
            },
            success: function (response) {
                if (response.success) {
                    // Очищаем поле ввода
                    $('#messageText').val('');
                    // Сообщение будет добавлено через SignalR (connection.on("ReceiveMessage"))
                    
                    // Сбрасываем состояние набора текста (уведомление отправится через SignalR)
                    resetTypingState(friendId); 
                    
                    console.log('Send message request successful.');
                } else {
                    toastr.error(response.error || 'Не удалось отправить сообщение (серверная ошибка)');
                }
            },
            error: function (xhr) {
                console.error('Send Message AJAX Error:', xhr.responseText);
                toastr.error('Ошибка сети при отправке сообщения');
            }
        });
    });

    // Функция инициализации индикатора набора текста
    function initializeTypingIndicator(friendId) {
        if (!friendId) return;

        $('#messageText').on('input', function() {
            isTyping = true;
            clearTimeout(typingTimer);

            const now = Date.now();
            if (!isUserTyping || now - lastTypingNotification > 3000) {
                notifyTyping(friendId, true); // Уведомляем через SignalR
                isUserTyping = true;
                lastTypingNotification = now;
            }

            typingTimer = setTimeout(function() {
                resetTypingState(friendId);
            }, 2000);
        });

        $('#messageText').on('blur', function() {
            resetTypingState(friendId);
        });
    }

    // Функция сброса состояния набора текста
    function resetTypingState(friendId) {
        isTyping = false;
        isUserTyping = false;
        notifyTyping(friendId, false); // Уведомляем через SignalR
    }

    // Функция отправки уведомления о наборе текста через SignalR
    async function notifyTyping(friendId, isTyping) {
        if (!friendId) return;
        try {
             await connection.invoke("NotifyTyping", friendId, isTyping);
        } catch (err) {
             console.error("Notify Typing Error: ", err);
        }
    }

    // Функция воспроизведения звука при получении сообщения
    function playMessageSound() {
        const audio = document.getElementById('message-sound');
        if (audio) {
            audio.play().catch(error => {
                console.error('Ошибка воспроизведения звука:', error);
            });
        }
    }
    
    // Функционал остановки/возобновления при смене видимости вкладки больше не нужен,
    // так как SignalR поддерживает соединение в фоне.
    // $(document).on('visibilitychange', function() { ... });
}); 