/**
 * Улучшенный скрипт переключения темы с устранением мерцания
 * и добавлением плавных анимаций
 */

// Константы
const STORAGE_KEY = 'theme-preference';
const DEFAULT_THEME = 'light';
const THEME_TOGGLE_ID = 'theme-toggle';
const BODY_SELECTOR = 'body';
const HTML_SELECTOR = 'html';
const THEME_ATTRIBUTE = 'data-theme';
const PRELOADER_ID = 'preloader';
const ANIMATION_CLASS = 'theme-fade-in';
const NO_JS_CLASS = 'no-js';
const PRELOADER_HIDDEN_CLASS = 'hidden';

// Задержка для предотвращения частых вызовов функции
const debounce = (fn, delay) => {
  let timer;
  return (...args) => {
    clearTimeout(timer);
    timer = setTimeout(() => fn(...args), delay);
  };
};

/**
 * Получает предпочтительную тему из localStorage или из системных настроек
 * @returns {string} тема ('light' или 'dark')
 */
const getThemePreference = () => {
  // Проверяем, сохранена ли тема в localStorage
  const savedTheme = localStorage.getItem(STORAGE_KEY);
  if (savedTheme) {
    return savedTheme;
  }
  
  // Проверяем системные настройки темы
  if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
    return 'dark';
  }
  
  // Возвращаем тему по умолчанию
  return DEFAULT_THEME;
};

/**
 * Устанавливает тему для документа
 * @param {string} theme - тема для установки ('light' или 'dark')
 */
const setTheme = (theme) => {
  // Получаем элемент body
  const body = document.querySelector(BODY_SELECTOR);
  
  // Устанавливаем атрибут темы на html (root element)
  document.documentElement.setAttribute(THEME_ATTRIBUTE, theme);
  
  // Сохраняем предпочтение темы в localStorage
  localStorage.setItem(STORAGE_KEY, theme);
  
  // Обновляем состояние переключателя темы, если он существует
  const themeToggle = document.getElementById(THEME_TOGGLE_ID);
  if (themeToggle) {
    themeToggle.checked = theme === 'dark';
    
    // Принудительно обновляем стили переключателя
    const sliderElement = document.querySelector('.slider');
    if (sliderElement) {
      // Удаляем и добавляем класс для сброса анимации
      sliderElement.classList.remove('slider-animation');
      void sliderElement.offsetWidth; // Форсируем reflow
      sliderElement.classList.add('slider-animation');
    }
  }
  
  // Добавляем класс анимации
  setTimeout(() => {
    body.classList.add(ANIMATION_CLASS);
    
    // Удаляем класс анимации после завершения
    setTimeout(() => {
      body.classList.remove(ANIMATION_CLASS);
    }, 300);
  }, 10);
};

/**
 * Переключает тему между светлой и темной
 */
const toggleTheme = () => {
  const currentTheme = getThemePreference();
  const newTheme = currentTheme === 'light' ? 'dark' : 'light';
  setTheme(newTheme);
};

/**
 * Инициализирует тему при загрузке страницы
 */
const initTheme = () => {
  // Устанавливаем тему до отображения DOM для предотвращения мерцания
  const theme = getThemePreference();
  document.documentElement.setAttribute(THEME_ATTRIBUTE, theme);
  
  // Удаляем класс no-js как только JavaScript загружен
  document.body.classList.remove(NO_JS_CLASS);
  document.documentElement.classList.remove(NO_JS_CLASS);
  
  // Добавляем слушатель события для переключателя темы
  document.addEventListener('DOMContentLoaded', () => {
    const themeToggle = document.getElementById(THEME_TOGGLE_ID);
    if (themeToggle) {
      themeToggle.checked = theme === 'dark';
      themeToggle.addEventListener('change', debounce(toggleTheme, 100));
    }
    
    // Обработка системного переключения темы
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
      const newTheme = e.matches ? 'dark' : 'light';
      setTheme(newTheme);
    });
    
    // Скрываем прелоадер после загрузки страницы
    const preloader = document.getElementById(PRELOADER_ID);
    if (preloader) {
      setTimeout(() => {
        preloader.classList.add(PRELOADER_HIDDEN_CLASS);
        setTimeout(() => {
          preloader.style.display = 'none';
        }, 300);
      }, 500);
    }
  });
};

// Инициализируем тему немедленно
initTheme();

// Удаляем экспорт, так как используем обычный скрипт, а не модуль
// export { getThemePreference, setTheme, toggleTheme }; 