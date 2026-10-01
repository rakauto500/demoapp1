// Minimal demo application used as the system under test.
// State lives in sessionStorage so each browser session starts clean.
const DemoApp = (() => {
  const USERS = { demo: "secret123", locked: "secret123" };
  const LOCKED = new Set(["locked"]);
  const SESSION_KEY = "demoapp.user";
  const TODOS_KEY = "demoapp.todos";

  function initLogin() {
    const form = document.getElementById("login-form");
    const error = document.getElementById("error");
    const showError = (msg) => { error.textContent = msg; error.hidden = false; };

    form.addEventListener("submit", (e) => {
      e.preventDefault();
      const username = form.username.value.trim();
      const password = form.password.value;
      if (!username) return showError("Username is required");
      if (!password) return showError("Password is required");
      if (USERS[username] !== password) return showError("Invalid username or password");
      if (LOCKED.has(username)) return showError("This account has been locked");
      sessionStorage.setItem(SESSION_KEY, username);
      // Simulate a slow backend so tests must use explicit waits.
      setTimeout(() => { window.location.href = "dashboard.html"; }, 300);
    });
  }

  function initDashboard() {
    const user = sessionStorage.getItem(SESSION_KEY);
    if (!user) { window.location.href = "index.html"; return; }
    document.getElementById("welcome-user").textContent = user;

    let todos = JSON.parse(sessionStorage.getItem(TODOS_KEY) || "[]");
    let filter = "all";
    const list = document.getElementById("todo-list");
    const input = document.getElementById("new-todo");
    const save = () => sessionStorage.setItem(TODOS_KEY, JSON.stringify(todos));

    function render() {
      list.innerHTML = "";
      todos
        .filter((t) => filter === "all" || (filter === "completed") === t.done)
        .forEach((t) => {
          const li = document.createElement("li");
          li.className = "todo" + (t.done ? " completed" : "");
          li.dataset.test = "todo-item";
          li.innerHTML = `
            <input type="checkbox" class="toggle" ${t.done ? "checked" : ""}>
            <span class="title"></span>
            <button class="delete" aria-label="Delete">&times;</button>`;
          li.querySelector(".title").textContent = t.title;
          li.querySelector(".toggle").addEventListener("change", () => { t.done = !t.done; save(); render(); });
          li.querySelector(".delete").addEventListener("click", () => { todos = todos.filter((x) => x !== t); save(); render(); });
          list.appendChild(li);
        });
      document.getElementById("todo-count").textContent = todos.filter((t) => !t.done).length;
    }

    document.getElementById("todo-form").addEventListener("submit", (e) => {
      e.preventDefault();
      const title = input.value.trim();
      if (!title) return;
      todos.push({ title, done: false });
      input.value = "";
      save();
      render();
    });

    document.querySelectorAll("[data-filter]").forEach((btn) =>
      btn.addEventListener("click", () => {
        filter = btn.dataset.filter;
        document.querySelectorAll("[data-filter]").forEach((b) => b.classList.toggle("active", b === btn));
        render();
      })
    );

    document.getElementById("logout-button").addEventListener("click", () => {
      sessionStorage.clear();
      window.location.href = "index.html";
    });

    render();
  }

  return { initLogin, initDashboard };
})();
