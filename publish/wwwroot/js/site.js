// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function toggleMenu() {
    document.getElementById("side-menu").classList.toggle("open");
        window.addEventListener("click", function (e) {
        const menu = document.getElementById("side-menu");
        const burger = document.querySelector(".burger");
        if (!menu.contains(e.target) && !burger.contains(e.target)) {
            menu.classList.remove("open");
        }
    });
}
