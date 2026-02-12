// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    const btn = document.getElementById("userMenuBtn");
    const dropdown = document.getElementById("userDropdown");

    if (btn && dropdown) {
        btn.addEventListener("click", function (e) {
            e.stopPropagation();
            dropdown.classList.toggle("show");
        });

        document.addEventListener("click", function () {
            dropdown.classList.remove("show");
        });
    }
    const menu = document.getElementById("side-menu");
    const burger = document.querySelector(".burger");

    if (menu && burger) {
        window.addEventListener("click", function (e) {
            if (!menu.contains(e.target) && !burger.contains(e.target)) {
                menu.classList.remove("open");
            }
        });
    }
});

function toggleMenu() {
    const menu = document.getElementById("side-menu");
    if (menu) {
        menu.classList.toggle("open");
    }
}
