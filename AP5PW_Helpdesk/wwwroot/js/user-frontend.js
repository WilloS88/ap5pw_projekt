// /wwwroot/users.users-frontend.js
const CONFIG = {
    MODE: "mock", // "mock" = čistý frontend přes localStorage, "api" = REST backend
    API_BASE: "/api/users", // až bude hotové API
    STORAGE_KEY: "users"
};

// ---------- Datová vrstva ----------
class MockStore {
    constructor(key) {
        this.key = key;
        if (!localStorage.getItem(this.key)) {
            localStorage.setItem(this.key, JSON.stringify([]));
        }
    }
    _read() { return JSON.parse(localStorage.getItem(this.key) || "[]"); }
    _write(data) { localStorage.setItem(this.key, JSON.stringify(data)); }

    async list() { return this._read(); }
    async get(id) { return this._read().find(u => String(u.Id) === String(id)) || null; }
    async create(payload) {
        const data = this._read();
        const newId = crypto.randomUUID ? crypto.randomUUID() : Date.now();
        const user = { Id: String(newId), ...payload };
        data.push(user); this._write(data);
        return user;
    }
    async update(id, payload) {
        const data = this._read();
        const i = data.findIndex(u => String(u.Id) === String(id));
        if (i === -1) throw new Error("User not found");
        data[i] = { ...data[i], ...payload, Id: String(id) };
        this._write(data);
        return data[i];
    }
    async remove(id) {
        const data = this._read();
        const next = data.filter(u => String(u.Id) !== String(id));
        this._write(next);
        return true;
    }
}

class ApiStore {
    constructor(base) { this.base = base; }
    async list() { const r = await fetch(this.base); return await r.json(); }
    async get(id) { const r = await fetch(`${this.base}/${id}`); return await r.json(); }
    async create(payload) {
        const r = await fetch(this.base, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(payload) });
        return await r.json();
    }
    async update(id, payload) {
        const r = await fetch(`${this.base}/${id}`, { method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify(payload) });
        return await r.json();
    }
    async remove(id) {
        await fetch(`${this.base}/${id}`, { method: "DELETE" });
        return true;
    }
}

const store = CONFIG.MODE === "api" ? new ApiStore(CONFIG.API_BASE) : new MockStore(CONFIG.STORAGE_KEY);

// ---------- UI pomocníci ----------
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const fmtDate = iso => iso ? ("" + iso).slice(0, 10) : "";

function showAlert(text, type = "success") {
    const box = $("#alert");
    box.className = `alert alert-${type}`;
    box.textContent = text;
    box.classList.remove("d-none");
    setTimeout(() => box.classList.add("d-none"), 2400);
}

function openModal(title, user = null) {
    $("#modal-title").textContent = title;
    $("#modal").classList.remove("d-none");
    $("#modal-backdrop").classList.remove("d-none");

    const form = $("#user-form");
    form.reset();
    form.elements["Id"].value = user?.Id ?? "";
    form.elements["FirstName"].value = user?.FirstName ?? "";
    form.elements["LastName"].value = user?.LastName ?? "";
    form.elements["Nickname"].value = user?.Nickname ?? "";
    form.elements["Email"].value = user?.Email ?? "";
    form.elements["Phone"].value = user?.Phone ?? "";
    form.elements["BirthDate"].value = user?.BirthDate ? fmtDate(user.BirthDate) : "";
    form.elements["FirstName"].focus();
}
function closeModal() {
    $("#modal").classList.add("d-none");
    $("#modal-backdrop").classList.add("d-none");
}

// ---------- Render tabulky ----------
async function renderTable() {
    const tbody = $("#users-tbody");
    const users = await store.list();

    if (!users.length) {
        tbody.innerHTML = `
      <tr><td colspan="7" class="text-center text-muted py-4">
        No users yet. Click <b>New user</b>.
      </td></tr>`;
        return;
    }

    tbody.innerHTML = users.map(u => `
    <tr data-id="${u.Id}">
      <td>${escapeHtml(u.FirstName)}</td>
      <td>${escapeHtml(u.LastName)}</td>
      <td>${escapeHtml(u.Nickname)}</td>
      <td>${escapeHtml(u.Email)}</td>
      <td>${escapeHtml(u.Phone ?? "")}</td>
      <td>${fmtDate(u.BirthDate)}</td>
      <td class="text-end">
        <button class="btn btn-sm btn-outline-primary btn-edit">Edit</button>
        <button class="btn btn-sm btn-outline-danger ms-1 btn-del">Delete</button>
      </td>
    </tr>
  `).join("");
}

function escapeHtml(s = "") {
    return String(s)
        .replaceAll("&", "&amp;").replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;").replaceAll('"', "&quot;").replaceAll("'", "&#39;");
}

// ---------- Eventy ----------
function bindEvents() {
    $("#btn-new").addEventListener("click", () => openModal("New user"));

    $("#btn-cancel").addEventListener("click", closeModal);
    $("#modal-backdrop").addEventListener("click", closeModal);
    document.addEventListener("keydown", e => {
        if (e.key === "Escape" && !$("#modal").classList.contains("d-none")) closeModal();
    });

    $("#user-form").addEventListener("submit", async (e) => {
        e.preventDefault();
        const f = e.currentTarget;
        const payload = {
            FirstName: f.elements["FirstName"].value.trim(),
            LastName: f.elements["LastName"].value.trim(),
            Nickname: f.elements["Nickname"].value.trim(),
            Email: f.elements["Email"].value.trim(),
            Phone: f.elements["Phone"].value.trim(),
            BirthDate: f.elements["BirthDate"].value ? new Date(f.elements["BirthDate"].value).toISOString() : null
        };

        const id = f.elements["Id"].value;
        try {
            if (id) {
                await store.update(id, payload);
                showAlert("Changes saved.");
            } else {
                await store.create(payload);
                showAlert("User created.");
            }
            closeModal();
            await renderTable();
        } catch (err) {
            console.error(err);
            showAlert("Operation failed.", "danger");
        }
    });

    // Delegace akcí v tabulce
    $("#users-tbody").addEventListener("click", async (e) => {
        const row = e.target.closest("tr[data-id]");
        if (!row) return;
        const id = row.getAttribute("data-id");

        if (e.target.matches(".btn-edit")) {
            const user = await store.get(id);
            openModal("Edit user", user);
        }
        if (e.target.matches(".btn-del")) {
            if (confirm("Delete this user?")) {
                try {
                    await store.remove(id);
                    await renderTable();
                    showAlert("User deleted.");
                } catch (err) {
                    console.error(err);
                    showAlert("Delete failed.", "danger");
                }
            }
        }
    });
}

// ---------- Init ----------
(async function init() {
    bindEvents();
    await renderTable();

    // seed data (jen poprvé)
    if (CONFIG.MODE === "mock" && (await store.list()).length === 0) {
        await store.create({ FirstName: "Alice", LastName: "Novak", Nickname: "ali", Email: "alice@example.com", Phone: "777111222", BirthDate: "1999-05-12T00:00:00.000Z" });
        await store.create({ FirstName: "Bob", LastName: "Kral", Nickname: "bk", Email: "bob@example.com", Phone: "777333444", BirthDate: "2001-10-01T00:00:00.000Z" });
        await renderTable();
    }
})();
