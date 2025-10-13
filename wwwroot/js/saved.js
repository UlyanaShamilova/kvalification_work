console.log("saved.js загружен и выполняется");


$(document).on("click", ".save-btn", function (e) {
    e.preventDefault(); // важно, чтобы не срабатывал переход по ссылке
    var button = $(this);
    var recipeId = button.data("id");
    var icon = button.find("img");

    if (icon.attr("src").includes("filled_star.jpg")) {
        // уже сохранён → удаляем
        $.post("/Home/UnsaveRecipe", { recipeId: recipeId }, function (res) {
            if (res.success) {
                icon.attr("src", "/images/empty_star.jpg");
            }
        });
    } else {
        // сохраняем
        $.post("/Home/SaveRecipe", { recipeId: recipeId }, function (res) {
            if (res.success) {
                icon.attr("src", "/images/filled_star.jpg");
            }
        });
    }
});
