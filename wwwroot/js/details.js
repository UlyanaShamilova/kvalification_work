document.getElementById('save-btn').addEventListener('click', function() {
    var recipeId = this.getAttribute('data-id');
    window.location.href = '/Home/DownloadRecipe/' + recipeId;
});

