// Функция для переключения темы
function toggleTheme() {
    // Получаем текущую тему
    const currentTheme = localStorage.getItem('theme') || 'light';
    
    // Определяем новую тему
    const newTheme = currentTheme === 'light' ? 'dark' : 'light';
    
    // Устанавливаем атрибут data-theme для body
    document.body.setAttribute('data-theme', newTheme);
    
    // Сохраняем тему в localStorage
    localStorage.setItem('theme', newTheme);
    
    // Обновляем состояние переключателя
    document.getElementById('theme-switch').checked = newTheme === 'dark';
}

// Функция инициализации темы при загрузке страницы
function initTheme() {
    // Получаем сохраненную тему из localStorage
    const savedTheme = localStorage.getItem('theme');
    
    // Если тема была сохранена, применяем её
    if (savedTheme) {
        document.body.setAttribute('data-theme', savedTheme);
        document.getElementById('theme-switch').checked = savedTheme === 'dark';
    }
}

// Инициализируем тему при загрузке страницы
document.addEventListener('DOMContentLoaded', function() {
    initTheme();
    
    // Добавляем обработчик события для переключателя
    const themeSwitch = document.getElementById('theme-switch');
    themeSwitch.addEventListener('change', toggleTheme);
}); 