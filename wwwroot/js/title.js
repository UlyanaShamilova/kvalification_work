document.addEventListener("DOMContentLoaded", function () {
    const arrowLink = document.querySelector('.arrow-link');

    if (arrowLink) {
        arrowLink.addEventListener('click', function (e) {
            e.preventDefault();

            document.body.classList.add('cube-flip');

            setTimeout(() => {
                window.location.href = arrowLink.href;
            }, 800); // время совпадает с анимацией
        });
    }
});
