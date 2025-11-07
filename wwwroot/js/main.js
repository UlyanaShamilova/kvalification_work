document.addEventListener('DOMContentLoaded', function () {
    const btn = document.getElementById('userMenuBtn');
    const dropdown = document.getElementById('userDropdown');

    btn.addEventListener('click', function (e) {
        e.stopPropagation();
        dropdown.style.display = dropdown.style.display === 'block' ? 'none' : 'block';
    });

    document.addEventListener('click', function () {
        dropdown.style.display = 'none';
    });
});

var categoryLinks = document.querySelectorAll(".category-link");
var recipes = document.querySelectorAll(".recipe-card");
var searchInput = document.getElementById("searchInput");
var searchBtn = document.getElementById("searchBtn");

var selectedCategory = "";

categoryLinks.forEach(function(link) {
    link.addEventListener("click", function() {
        selectedCategory = this.getAttribute("data-category");
        filterRecipes();
    });
});

searchBtn.addEventListener("click", function() {
    filterRecipes();
});


function normalizeWord(word) {
    word = word.toLowerCase().trim();
    
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
    var queryRoot = normalizeWord(query);

    recipes.forEach(function(recipe) {
        var name = recipe.querySelector(".recipe-name").textContent.toLowerCase();
        var ingredientsText = recipe.querySelector(".recipe-ingredients") ? 
                              recipe.querySelector(".recipe-ingredients").textContent.toLowerCase() : "";
        var category = recipe.getAttribute("data-category");

        var ingredientsArr = ingredientsText.split(',').map(i => normalizeWord(i));

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

function toggleChat() {
    const chat = document.getElementById("chat-window");
    chat.style.display = (chat.style.display === "flex") ? "none" : "flex";
}

function appendMessage(who, text) {
  var messages = document.getElementById("chat-messages");
  if (!messages) {
    return;
  }

  var wrapper = document.createElement("div");
  wrapper.className = (who === "user") ? "msg user-msg" : "msg bot-msg";

  var bold = document.createElement("b");
  bold.textContent = (who === "user") ? "Ви: " : "ШІ: ";

  var span = document.createElement("span");
  span.textContent = text;

  wrapper.appendChild(bold);
  wrapper.appendChild(span);

  messages.appendChild(wrapper);

  messages.scrollTop = messages.scrollHeight;
}








async function sendMessage() {
    const input = document.getElementById('chat-text');
    const message = input.value.trim();
    if (!message) return;

    addMessageToChat('Ви', message);
    input.value = '';

    try {
        const response = await fetch('/api/ai/chat', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Message: message })
        });

        if (!response.ok) {
            const text = await response.text();
            console.error('HTTP error:', response.status, text);
            addMessageToChat('Error', `Помилка сервера: ${response.status}`);
            return;
        }

        const data = await response.json();
        addMessageToChat('Асистент', data.reply);
    } catch (error) {
        console.error('Fetch error:', error);
        addMessageToChat('Error', 'Щось пішло не так...');
    }
}

function addMessageToChat(sender, message) {
    const chatMessages = document.getElementById('chat-messages');
    const messageElem = document.createElement('div');
    messageElem.classList.add('chat-message');
    messageElem.textContent = `${sender}: ${message}`;
    chatMessages.appendChild(messageElem);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}
