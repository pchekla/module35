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
