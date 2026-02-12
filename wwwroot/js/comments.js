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

    $.ajax({
        url: '/Comments/Add',
        type: 'POST',
        data: $(this).serialize(),
        success: function(result){

            let userComment = $("#comment_text").val().trim().toLowerCase();

            let $newComment = $(result);
            $("#comments").append($newComment);
            $("#comment_text").val('');

            if (isPositiveComment(userComment)) {

                let animation = `
                    <div class="good-comment-animation">
                        <img src="/uploads/1f44b.gif" alt="good">
                        <p class="good-comm">Дякуємо за гарний коментар!</p>
                    </div>
                `;

                $newComment.append(animation);

                let $anim = $newComment.find(".good-comment-animation");
                $anim.fadeIn(300);

                setTimeout(() => {
                    $anim.fadeOut(300, function () {
                        $(this).remove();
                    });
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
    const wordsList = ["смачно", "клас", "круто", "шикарно", "супер", "топ", "найкраще", "рекомендую", "обожнюю", "сподобалось"];

    text = text.toLowerCase();

    return wordsList.some(word => text.includes(word));
}

$(document).on("click", ".edit-btn", function(e){
    e.preventDefault();
    var commentDiv = $(this).closest(".comment");
    var commentText = commentDiv.find(".comment-text").text().trim();
    var commentId = $(this).data("id");

    commentDiv.find(".comment-text").hide();
    $(this).hide();

    var editForm = `
        <form class="editForm">
            <textarea name="text" required>${commentText}</textarea>
            <button type="submit">Зберегти</button>
            <button type="button" class="cancelEdit">Скасувати</button>
        </form>
    `;
    commentDiv.append(editForm);
});

$(document).on("click", ".cancelEdit", function(){
    var form = $(this).closest(".editForm");
    var commentDiv = $(this).closest(".comment");

    commentDiv.find(".comment-text").show();
    commentDiv.find(".edit-btn").show();
    form.remove();
});

$(document).on("submit", ".editForm", function(e){
    e.preventDefault();
    var form = $(this);
    var commentDiv = form.closest(".comment");
    var commentId = commentDiv.find(".edit-btn").data("id");

    $.ajax({
        url: '/Comments/Edit',
        type: 'POST',
        data: {
            id: commentId,
            text: form.find("textarea[name='text']").val()
        },
        success: function(result){
            commentDiv.replaceWith(result);
        },
        error: function(){
            alert("Не вдалося редагувати коментар");
        }
    });
});