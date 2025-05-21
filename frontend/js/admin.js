document.addEventListener('DOMContentLoaded', async () => {
    // Проверка прав доступа
    const token = localStorage.getItem('token');
    const role = localStorage.getItem('role');
    
    if (!token || role !== 'Admin') {
        window.location.href = 'index.html';
        return;
    }

    // Загрузка списка пользователей
    const loadUsers = async () => {
        try {
            const response = await fetch('http://localhost:5281/api/auth/users', {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            
            if (!response.ok) throw new Error('Ошибка загрузки');
            
            const users = await response.json();
            renderUsers(users);
        } catch (error) {
            alert(error.message);
        }
    };

    // Рендер таблицы
    const renderUsers = (users) => {
        const tbody = document.getElementById('usersTable');
        tbody.innerHTML = users.map(user => `
            <tr>
                <td>${user.username}</td>
                <td>${user.email}</td>
                <td><span class="badge bg-${user.role === 'Admin' ? 'primary' : 'secondary'}">${user.role}</span></td>
                <td>${new Date(user.createdAt).toLocaleString()}</td>
                <td>
                    <button class="btn btn-danger btn-sm" 
                            onclick="handleDelete('${user.id}')">
                        Удалить
                    </button>
                </td>
            </tr>
        `).join('');
    };

    // Обработчик удаления
    window.handleDelete = async (userId) => {
        if (!confirm('Вы уверены, что хотите удалить пользователя?')) return;
        
        try {
            const response = await fetch(`http://localhost:5281/api/auth/users/${userId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            
            if (!response.ok) throw new Error('Ошибка удаления');
            
            await loadUsers();
            alert('Пользователь успешно удален');
        } catch (error) {
            alert(error.message);
        }
    };

    // Обработчик добавления
    document.getElementById('addUserForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const newUser = {
            username: document.getElementById('addUsername').value,
            email: document.getElementById('addEmail').value,
            password: document.getElementById('addPassword').value,
            role: document.getElementById('addRole').value
        };

        try {
            const response = await fetch('http://localhost:5281/api/auth/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(newUser)
            });

            const data = await response.json();
            
            if (!response.ok) throw new Error(data.message || 'Ошибка регистрации');
            
            alert('Пользователь успешно создан!');
            document.getElementById('addUserModal').querySelector('.btn-close').click();
            await loadUsers();
        } catch (error) {
            alert(error.message);
        }
    });

    await loadUsers();
});