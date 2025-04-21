// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Обработка общих функций для всего сайта
document.addEventListener('DOMContentLoaded', function() {
    // Инициализация подсказок Bootstrap
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Инициализация всплывающих окон Bootstrap
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
    
    // Эффекты hover для изображений
    const hoverImages = document.querySelectorAll('.image-hover');
    hoverImages.forEach(img => {
        img.addEventListener('mouseover', function() {
            this.classList.add('image-hover-active');
        });
        
        img.addEventListener('mouseout', function() {
            this.classList.remove('image-hover-active');
        });
    });
    
    // Анимация появления блоков при прокрутке
    const animateElements = document.querySelectorAll('.animate-on-scroll');
    
    function checkScroll() {
        animateElements.forEach(element => {
            const elementPosition = element.getBoundingClientRect().top;
            const screenPosition = window.innerHeight / 1.2;
            
            if (elementPosition < screenPosition) {
                element.classList.add('animate-active');
            }
        });
    }
    
    // Проверяем позицию при загрузке страницы
    checkScroll();
    
    // И при прокрутке
    window.addEventListener('scroll', checkScroll);
});

// Функция для обработки отправки сообщений в чате без перезагрузки страницы
function setupChatForm() {
    const form = document.getElementById('messageForm');
    const input = document.getElementById('messageInput');
    const sendButton = document.getElementById('sendButton');
    const chatMessages = document.getElementById('chatMessages');

    if (!form || !input || !sendButton || !chatMessages) {
        return; // Если элементы не найдены, выходим из функции
    }

    function createMessageElement(text, isFromCurrentUser = true) {
        const now = new Date();
        const formattedTime = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}, ${now.getDate()} ${['янв', 'фев', 'мар', 'апр', 'май', 'июн', 'июл', 'авг', 'сен', 'окт', 'ноя', 'дек'][now.getMonth()]}`;
        
        const messageDiv = document.createElement('div');
        messageDiv.className = `mb-3 ${isFromCurrentUser ? 'text-end' : ''}`;
        
        messageDiv.innerHTML = `
            <div class="d-inline-block message-bubble ${isFromCurrentUser ? 'bg-primary text-white' : 'bg-light'}">
                <div class="message-content">${text}</div>
                <div class="message-time ${isFromCurrentUser ? 'text-white-50' : 'text-muted'}">${formattedTime}</div>
            </div>
        `;
        
        return messageDiv;
    }

    // Добавляем обработчик формы
    form.addEventListener('submit', function(e) {
        e.preventDefault();
        
        const messageText = input.value.trim();
        if (!messageText) {
            return false;
        }
        
        // Блокируем кнопку отправки
        sendButton.disabled = true;
        sendButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Отправка...';
        
        // Отправляем данные формы через AJAX
        const formData = new FormData(form);
        const url = form.getAttribute('action');
        
        fetch(url, {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            credentials: 'same-origin' // Добавляем для передачи куки, включая токен аутентификации
        })
        .then(response => {
            if (response.ok) {
                return response.json(); // Парсим JSON-ответ
            } else {
                throw new Error('Ошибка сети');
            }
        })
        .then(data => {
            if (data.success) {
                // Очищаем поле ввода
                input.value = '';
                
                // Добавляем сообщение в чат
                const messageElement = createMessageElement(messageText, true);
                chatMessages.appendChild(messageElement);
                
                // Скроллим чат вниз
                chatMessages.scrollTop = chatMessages.scrollHeight;
                
                // Показываем успешное сообщение
                if (typeof toastr !== 'undefined') {
                    toastr.success('Сообщение отправлено');
                }
                
                console.log('Сообщение успешно сохранено в БД:', data.message);
            } else {
                console.error('Ошибка сохранения в БД:', data.error);
                if (typeof toastr !== 'undefined') {
                    toastr.error(data.error || 'Ошибка при отправке сообщения');
                }
            }
        })
        .catch(error => {
            console.error('Ошибка при отправке:', error);
            if (typeof toastr !== 'undefined') {
                toastr.error('Ошибка сети при отправке сообщения');
            }
        })
        .finally(() => {
            // Разблокируем кнопку
            sendButton.disabled = false;
            sendButton.innerHTML = '<i class="bi bi-send"></i> Отправить';
        });
        
        return false;
    });
}

// Инициализируем форму чата при загрузке страницы
document.addEventListener('DOMContentLoaded', function() {
    setupChatForm();
});
