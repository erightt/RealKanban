
function updateNavbar() {
    const authButtons = document.getElementById('authButtons');
    const token = localStorage.getItem('token');
    
    if (token) {
        const username = localStorage.getItem('username');
        authButtons.innerHTML = `
            <span class="nav-link">${username}</span>
            <button class="btn btn-link nav-link" onclick="logout()">Выход</button>
        `;
    } else {
        authButtons.innerHTML = `
            <a class="nav-link" href="login.html">Вход</a>
            <a class="nav-link" href="register.html">Регистрация</a>
        `;
    }
}

function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    localStorage.removeItem('username');
    window.location.href = 'index.html';
}

document.addEventListener('DOMContentLoaded', () => {
    updateNavbar();
    

    if (window.location.pathname.includes('dashboard.html')) {
        const token = localStorage.getItem('token');
        if (!token) {
            window.location.href = 'index.html';
        } else {
            document.getElementById('username').textContent = localStorage.getItem('username');
        }
    }
});

if (document.getElementById('loginForm')) {
    document.getElementById('loginForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const response = await fetch('http://localhost:5281/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                email: document.getElementById('email').value,
                password: document.getElementById('password').value
            })
        });

        const data = await response.json();
        if (response.ok) {
            localStorage.setItem('token', data.token);
            localStorage.setItem('userId', data.userId);
            localStorage.setItem('username', data.username);
            window.location.href = 'dashboard.html';
        } else {
            alert(data.message || 'Ошибка входа');
        }
    });
}

if (document.getElementById('registerForm')) {
    document.getElementById('registerForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const password = document.getElementById('password').value;
        const confirmPassword = document.getElementById('confirmPassword').value;

        if (password !== confirmPassword) {
            alert('Пароли не совпадают!');
            return;
        }

        const response = await fetch('http://localhost:5281/api/auth/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                username: document.getElementById('username').value,
                email: document.getElementById('email').value,
                password: password,
                role: 'User'
            })
        });

        const data = await response.json();
        if (response.ok) {
            alert('Регистрация успешна! Теперь войдите.');
            window.location.href = 'login.html';
        } else {
            alert(data.message || 'Ошибка регистрации');
        }
    });
}