$(document).on("click", "#btn-reply", function(e){
    e.preventDefault();
    var commentId = $(this).data("id");
    var formHtml = `
        <form class="replyForm">
            <input type="hidden" name="recipeID" value="${$("#recipeId").val()}">
            <input type="hidden" name="parentID" value="${commentId}">
            <textarea name="text" required></textarea>
            <button type="submit" style="background-color: rgb(121, 100, 32); color: white; font-family: Georgia, 'Times New Roman', serif; font-size: 13px; font-weight: 600; 
            border-radius: 8px; transition: background 0.3s ease, transform 0.2s ease; width: 100px;height: 30px; text-align: center;">Відправити</button>
        </form>`;
    
    $(this).siblings(".replies").first().append(formHtml);
});

$(document).on("submit", ".replyForm", function(e){
    e.preventDefault();
    var form = $(this);

    $.ajax({
        url: '/Comments/Add',
        type: 'POST',
        data: form.serialize(),
        success: function(result){
            form.replaceWith(result);
        },
        error: function(){
            alert("Сталася помилка при додаванні відповіді");
        }
    });
});

$(document).on("submit", "#commentForm", function(e){
    e.preventDefault();
    var form = $(this);

    $.ajax({
        url: '/Comments/Add',
        type: 'POST',
        data: form.serialize(),
        success: function(result){
    let userComment = $("#comment_text").val().toLowerCase();

    $("#comments").append(result);
    $("#comment_text").val('');

    if (isPositiveComment(userComment)) {
        $("#goodCommentAnimation").fadeIn(300);

        setTimeout(() => {
            $("#goodCommentAnimation").fadeOut(300);
        }, 2500);
    }
},
        error: function(){
            alert("Сталася помилка при додаванні коментаря");
        }
    });
});

$(document).on("click", ".delete-btn", function(e){
    e.preventDefault();
    if(!confirm("Ви впевнені, що хочете видалити цей коментар?")) return;

    var commentId = $(this).data("id");
    var commentDiv = $(this).closest(".comment");

    $.ajax({
        url: '/Comments/Delete',
        type: 'POST',
        data: { id: commentId },
        success: function(){
            commentDiv.remove();
        },
        error: function(){
            alert("Не вдалося видалити коментар");
        }
    });
});

function isPositiveComment(text) {
    const positiveWords = ["смачно", "клас", "круто", "шикарно", "супер", "топ", "найкраще", "рекомендую", "обожнюю", "сподобалось", "мені сподобалось"];

    text = text.toLowerCase();

    return positiveWords.some(word => text.includes(word));
}