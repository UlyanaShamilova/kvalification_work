// $(document).on("click", ".reply-btn", function(e){
//     e.preventDefault();
//     var commentId = $(this).data("id");
//     var formHtml = `
//         <form class="replyForm">
//             <input type="hidden" name="recipeID" value="${$("#recipeId").val()}">
//             <input type="hidden" name="parentID" value="${commentId}">
//             <textarea name="text" required></textarea>
//             <button type="submit">Відправити</button>
//         </form>`;
    
//     $(this).siblings(".replies").first().append(formHtml);
// });

// $(document).on("submit", ".replyForm", function(e){
//     e.preventDefault();
//     var form = $(this);

//     $.ajax({
//         url: '/Comments/Add',
//         type: 'POST',
//         data: form.serialize(),
//         success: function(result){
//             form.replaceWith(result); // заменяем форму на сам комментарий
//         },
//         error: function(){
//             alert("Сталася помилка при додаванні відповіді");
//         }
//     });
// });

// $(document).on("submit", "#commentForm", function(e){
//     e.preventDefault();
//     var form = $(this);

//     $.ajax({
//         url: '/Comments/Add',
//         type: 'POST',
//         data: form.serialize(),
//         success: function(result){
//             $("#comments").append(result); // добавляем новый комментарий в конец
//             $("#comment_text").val(''); // очищаем textarea
//         },
//         error: function(){
//             alert("Сталася помилка при додаванні коментаря");
//         }
//     });
// });

// $(document).on("click", ".delete-btn", function(e){
//     e.preventDefault();
//     if(!confirm("Ви впевнені, що хочете видалити цей коментар?")) return;

//     var commentId = $(this).data("id");
//     var commentDiv = $(this).closest(".comment");

//     $.ajax({
//         url: '/Comments/Delete',
//         type: 'POST',
//         data: { id: commentId },
//         success: function(){
//             commentDiv.remove(); // удаляем блок с комментарием
//         },
//         error: function(){
//             alert("Не вдалося видалити коментар");
//         }
//     });
// });


$(document).on("click", ".reply-btn", function(e){
    e.preventDefault();
    var commentId = $(this).data("id");
    var formHtml = `
        <form class="replyForm">
            <input type="hidden" name="recipeID" value="${$("#recipeId").val()}">
            <input type="hidden" name="parentID" value="${commentId}">
            <textarea name="text" required></textarea>
            <button type="submit">Відправити</button>
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
            form.replaceWith(result); // заменяем форму на сам комментарий
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
            $("#comments").append(result); // добавляем новый комментарий в конец
            $("#comment_text").val('');    // очищаем textarea
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
            commentDiv.remove(); // удаляем блок с комментарием
        },
        error: function(){
            alert("Не вдалося видалити коментар");
        }
    });
});
