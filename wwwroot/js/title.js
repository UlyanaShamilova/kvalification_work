document.addEventListener("DOMContentLoaded", function () {
    const arrow = document.getElementById("arrow_img");
    if (arrow) {
        arrow.addEventListener("click", function (e) {
            e.preventDefault();
            document.body.classList.add("page-transition");

            const targetUrl = "/Home/main_page";
            setTimeout(() => {
                window.location.href = targetUrl;
            }, 1000);
        });
    }
});