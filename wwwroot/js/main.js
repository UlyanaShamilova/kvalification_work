document.addEventListener('DOMContentLoaded', function () {
    const btn = document.getElementById('userMenuBtn');
    const dropdown = document.getElementById('userDropdown');

    btn.addEventListener('click', function (e) {
        e.stopPropagation(); // чтобы клик не закрывал меню сразу
        dropdown.style.display = dropdown.style.display === 'block' ? 'none' : 'block';
    });

    // Закрываем дропдаун, если клик вне него
    document.addEventListener('click', function () {
        dropdown.style.display = 'none';
    });
});



var categoryLinks = document.querySelectorAll(".category-link");
var recipes = document.querySelectorAll(".recipe-card");
var searchInput = document.getElementById("searchInput");
var searchBtn = document.getElementById("searchBtn");

var selectedCategory = "";

// Фильтр по категориям
categoryLinks.forEach(function(link) {
    link.addEventListener("click", function() {
        selectedCategory = this.getAttribute("data-category");
        filterRecipes();
    });
});

// Фильтр по поиску при клике на кнопку
searchBtn.addEventListener("click", function() {
    filterRecipes();
});


function normalizeWord(word) {
    word = word.toLowerCase().trim();
    
    // Убираем распространённые окончания
    const endings = ["ів", "и", "а", "и", "я", "ів", "ів", "у", "е", "и", "ї", "і"];
    for (let ending of endings) {
        if (word.endsWith(ending)) {
            return word.slice(0, -ending.length);
        }
    }
    return word;
}


function filterRecipes() {
    var query = searchInput.value.toLowerCase().trim();
    var queryRoot = normalizeWord(query); // получаем “корень” запроса

    recipes.forEach(function(recipe) {
        var name = recipe.querySelector(".recipe-name").textContent.toLowerCase();
        var ingredientsText = recipe.querySelector(".recipe-ingredients") ? 
                              recipe.querySelector(".recipe-ingredients").textContent.toLowerCase() : "";
        var category = recipe.getAttribute("data-category");

        var ingredientsArr = ingredientsText.split(',').map(i => normalizeWord(i));

        // проверяем, есть ли в ингредиентах слово с таким корнем
        var matchesIngredients = ingredientsArr.some(ing => ing.includes(queryRoot));

        var matchesCategory = !selectedCategory 
                              || selectedCategory === "всі рецепти" 
                              || category === selectedCategory;

        var matchesQuery = !query || name.includes(query) || matchesIngredients;

        if (matchesCategory && matchesQuery) {
            recipe.style.display = "inline-block";
        } else {
            recipe.style.display = "none";
        }
    });
}

/* Переключатель видимости окна чата */
function toggleChat() {
    const chat = document.getElementById("chat-window");
    chat.style.display = (chat.style.display === "flex") ? "none" : "flex";
}

/* Добавляет сообщение в область сообщений */
function appendMessage(who, text) {
  var messages = document.getElementById("chat-messages");
  if (!messages) {
    return;
  }

  // контейнер для одного сообщения
  var wrapper = document.createElement("div");
  wrapper.className = (who === "user") ? "msg user-msg" : "msg bot-msg";

  var bold = document.createElement("b");
  bold.textContent = (who === "user") ? "Ви: " : "ШІ: ";

  var span = document.createElement("span");
  span.textContent = text;

  wrapper.appendChild(bold);
  wrapper.appendChild(span);

  messages.appendChild(wrapper);
  // прокрутить вниз
  messages.scrollTop = messages.scrollHeight;
}

/* Отправляет запрос на бэкенд и обрабатывает ответ */
function postToBackend(url, bodyObject, onSuccess, onError) {
  fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(bodyObject)
  })
  .then(function (response) {
    if (!response.ok) {
      throw new Error("Network response was not ok: " + response.status);
    }
    return response.json();
  })
  .then(function (data) {
    if (typeof onSuccess === "function") {
      onSuccess(data);
    }
  })
  .catch(function (error) {
    if (typeof onError === "function") {
      onError(error);
    } else {
      console.error("Fetch error:", error);
    }
  });
}

/* Основная функция отправки сообщения */
function sendMessage() {
  var input = document.getElementById("chat-text");
  if (!input) {
    return;
  }
  var raw = input.value || "";
  var msg = raw.trim();
  if (!msg) {
    return;
  }

  // Показываем сообщение пользователя в чате
  appendMessage("user", msg);

  // Подготовим тело запроса.
  // Здесь старый подход: разделяем по пробелам на "ингредиенты".
  // Ты можешь поменять логику разбора на более сложную (через запятые и т.д.).
  var ingredientsArray = msg.split(/\s+/);

  // Отправляем на бэкенд
  postToBackend("http://localhost:5252/api/chat/ask", { question: msg },
    function (data) {
      // onSuccess — обработаем ответ
      // Возможны варианты формата ответа: { answer: "..." } или массив рецептов
      if (data === null || data === undefined) {
        appendMessage("bot", "Пустой ответ от сервера.");
        return;
      }

      // Если сервер вернул объект с полем answer
      if (typeof data === "object" && data.answer) {
        appendMessage("bot", String(data.answer));
        return;
      }

      // Если сервер вернул массив рецептов
      if (Array.isArray(data) && data.length > 0) {
        // Соберём короткий текст с названиями / инструкциями
        var out = "";
        for (var i = 0; i < data.length; i++) {
          var r = data[i];
          // ожидаем, что у рецепта есть title и instructions
          var title = r.title || ("Рецепт " + (i + 1));
          var instr = r.instructions || "";
          out += title + ": " + instr;
          if (i < data.length - 1) {
            out += " \n---\n ";
          }
        }
        appendMessage("bot", out);
        return;
      }

      // Иначе просто выводим сериализованный ответ
      try {
        appendMessage("bot", JSON.stringify(data));
      } catch (e) {
        appendMessage("bot", String(data));
      }
    },
    function (error) {
      appendMessage("bot", "Ошибка при запросе к серверу: " + error.message);
    }
  );

// Очистим поле ввода
  input.value = "";
}