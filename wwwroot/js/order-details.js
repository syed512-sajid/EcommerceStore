// Example: future enhancements for timeline animations
document.addEventListener('DOMContentLoaded', function () {
    const timelineItems = document.querySelectorAll('.timeline-item');

    timelineItems.forEach(item => {
        if (item.classList.contains('active')) {
            item.style.transform = 'translateX(0)';
            item.style.opacity = 1;
        } else {
            item.style.transform = 'translateX(-20px)';
            item.style.opacity = 0.5;
        }
    });
});
