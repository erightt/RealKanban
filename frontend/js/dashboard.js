console.log('Элементы модального окна:', {
    columnModal: document.getElementById('columnModal'),
    title: document.getElementById('columnModalTitle'),
    input: document.getElementById('columnName')
});
document.addEventListener('DOMContentLoaded', async () => {
    // Проверка авторизации
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = 'login.html';
        return;
    }

    // Инициализация модальных окон
    const boardModal = new bootstrap.Modal(document.getElementById('boardModal'));
    const columnModal = new bootstrap.Modal(document.getElementById('columnModal'));
    const cardModal = new bootstrap.Modal(document.getElementById('cardModal'));

    // Текущая выбранная доска
    let currentBoardId = null;
    let columns = [];
    let boards = [];
    let currentEditMode = { type: null, id: null }; // Текущий режим редактирования

    // Элементы интерфейса
    const boardsList = document.getElementById('boardsList');
    const kanbanContainer = document.getElementById('kanbanContainer');
    const currentBoardTitle = document.getElementById('currentBoardTitle');
    const addBoardBtn = document.getElementById('addBoardBtn');
    const addColumnBtn = document.getElementById('addColumnBtn');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const boardSearch = document.getElementById('boardSearch');

    // Инициализация Sortable для перетаскивания карточек
    let sortableColumns = [];

    // Инициализация обработчиков форм
    const initFormHandlers = () => {
        // Обработчик формы доски
        document.getElementById('boardForm').onsubmit = async (e) => {
            e.preventDefault();
            const name = document.getElementById('boardName').value;

            try {
                if (currentEditMode.type === 'board') {
                    // Редактирование существующей доски
                    await fetch(`http://localhost:5281/api/boards/${currentEditMode.id}`, {
                        method: 'PUT',
                        headers: {
                            'Content-Type': 'application/json',
                            'Authorization': `Bearer ${token}`
                        },
                        body: JSON.stringify(name)
                    });
                } else {
                    // Создание новой доски
                    const response = await fetch('http://localhost:5281/api/boards', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'Authorization': `Bearer ${token}`
                        },
                        body: JSON.stringify(name)
                    });
                    const board = await response.json();
                    currentBoardId = board.id;
                }

                boardModal.hide();
                await loadBoards();
                currentEditMode = { type: null, id: null };
            } catch (error) {
                console.error('Ошибка:', error);
                alert(error.message);
            }
        };

        // Обработчик формы колонки
        document.getElementById('columnForm').onsubmit = async (e) => {
            e.preventDefault();
            const name = document.getElementById('columnName').value;

            try {
                if (currentEditMode.type === 'column') {
                    // Редактирование существующей колонки
                    await fetch(`http://localhost:5281/api/boards/${currentBoardId}/columns/${currentEditMode.id}`, {
                        method: 'PUT',
                        headers: {
                            'Content-Type': 'application/json',
                            'Authorization': `Bearer ${token}`
                        },
                        body: JSON.stringify(name)
                    });
                } else {
                    // Создание новой колонки
                    await fetch(`http://localhost:5281/api/boards/${currentBoardId}/columns`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'Authorization': `Bearer ${token}`
                        },
                        body: JSON.stringify(name)
                    });
                }

                columnModal.hide();
                await loadColumns(currentBoardId);
                currentEditMode = { type: null, id: null };
            } catch (error) {
                console.error('Ошибка:', error);
                alert(error.message);
            }
        };

        // Обработчик формы карточки
        document.getElementById('cardForm').onsubmit = async (e) => {
            e.preventDefault();
            const title = document.getElementById('cardTitle').value;
            const description = document.getElementById('cardDescription').value;
            const columnId = document.getElementById('cardColumnId').value;

            try {
                if (currentEditMode.type === 'card') {
                    // Редактирование существующей карточки
                    await fetch(`http://localhost:5281/api/columns/${columnId}/cards/${currentEditMode.id}`, {
                        method: 'PUT',
                        headers: {
                            'Content-Type': 'application/json',
                            'Authorization': `Bearer ${token}`
                        },
                        body: JSON.stringify({
                            title: title,
                            description: description
                        })
                    });
                } else {
                    // Создание новой карточки
                    await fetch(`http://localhost:5281/api/columns/${columnId}/cards`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'Authorization': `Bearer ${token}`
                        },
                        body: JSON.stringify({
                            title: title,
                            description: description
                        })
                    });
                }

                cardModal.hide();
                await loadColumns(currentBoardId);
                currentEditMode = { type: null, id: null };
            } catch (error) {
                console.error('Ошибка:', error);
                alert(error.message);
            }
        };
    };

    // Загрузка досок пользователя
    const loadBoards = async () => {
        try {
            const response = await fetch('http://localhost:5281/api/boards', {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Ошибка загрузки досок');

            boards = await response.json();
            renderBoards();

            // Если есть доски, выбираем первую
            if (boards.length > 0 && !currentBoardId) {
                selectBoard(boards[0].id);
            }
        } catch (error) {
            console.error('Ошибка:', error);
            alert(error.message);
        }
    };

    // Рендер списка досок
    const renderBoards = () => {
        boardsList.innerHTML = boards.map(board => `
            <li class="nav-item board-item">
                <a href="#" class="nav-link ${board.id === currentBoardId ? 'active' : ''}" 
                   onclick="selectBoard('${board.id}')">
                    <span>${board.name}</span>
                    <div class="board-actions">
                        <button class="btn btn-sm btn-outline-secondary me-1" 
                                onclick="editBoard('${board.id}', event)">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-danger" 
                                onclick="deleteBoard('${board.id}', event)">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </a>
            </li>
        `).join('');
    };

    // Выбор доски
    window.selectBoard = async (boardId) => {
        currentBoardId = boardId;
        renderBoards();

        const board = boards.find(b => b.id === boardId);
        currentBoardTitle.textContent = board.name;
        addColumnBtn.disabled = false;

        await loadColumns(boardId);
    };

    // Загрузка колонок для выбранной доски
    const loadColumns = async (boardId) => {
        try {
            const response = await fetch(`http://localhost:5281/api/boards/${boardId}/columns`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Ошибка загрузки колонок');

            columns = await response.json();
            renderColumns();
        } catch (error) {
            console.error('Ошибка:', error);
            alert(error.message);
        }
    };

    // Рендер колонок и карточек
    const renderColumns = () => {
        kanbanContainer.innerHTML = '';

        if (columns.length === 0) {
            kanbanContainer.innerHTML = `
                <div class="empty-state text-center py-5">
                    <i class="bi bi-columns-gap text-muted" style="font-size: 3rem;"></i>
                    <h4 class="mt-3">Нет колонок</h4>
                    <p class="text-muted">Создайте первую колонку, нажав на кнопку выше</p>
                </div>
            `;
            return;
        }

        columns.forEach(column => {
            const columnElement = document.createElement('div');
            columnElement.className = 'kanban-column';
            columnElement.dataset.columnId = column.id;

            columnElement.innerHTML = `
                <div class="kanban-column-header">
                    <h3 class="kanban-column-title">${column.name}</h3>
                    <div class="kanban-column-actions">
                        <button class="btn btn-sm btn-outline-secondary me-1" 
                                onclick="editColumn('${column.id}', event)">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-danger me-1" 
                                onclick="deleteColumn('${column.id}', event)">
                            <i class="bi bi-trash"></i>
                        </button>
                        <button class="btn btn-sm btn-primary" 
                                onclick="showAddCardModal('${column.id}')">
                            <i class="bi bi-plus"></i>
                        </button>
                    </div>
                </div>
                <div class="kanban-cards" id="cards-${column.id}">
                    <!-- Карточки будут загружены через JS -->
                </div>
            `;

            kanbanContainer.appendChild(columnElement);

            // Загружаем карточки для колонки
            loadCards(column.id);
        });

        // Инициализация перетаскивания карточек
        initSortable();
    };

    // Загрузка карточек для колонки
    const loadCards = async (columnId) => {
        try {
            const response = await fetch(`http://localhost:5281/api/columns/${columnId}/cards`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Ошибка загрузки карточек');

            const cards = await response.json();
            renderCards(columnId, cards);
        } catch (error) {
            console.error('Ошибка:', error);
            alert(error.message);
        }
    };

    // Рендер карточек в колонке
    const renderCards = (columnId, cards) => {
        const cardsContainer = document.getElementById(`cards-${columnId}`);

        if (cards.length === 0) {
            cardsContainer.innerHTML = `
                <div class="text-center py-3 text-muted">
                    <i class="bi bi-card-text"></i>
                    <p class="mb-0">Нет карточек</p>
                </div>
            `;
            return;
        }

        cardsContainer.innerHTML = cards.map(card => `
            <div class="kanban-card" data-card-id="${card.id}">
                <h5 class="kanban-card-title">${card.title}</h5>
                ${card.description ? `<p class="kanban-card-description">${card.description}</p>` : ''}
                <div class="d-flex justify-content-end">
                    <button class="btn btn-sm btn-outline-secondary me-1" 
                            onclick="editCard('${card.id}', '${columnId}', event)">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" 
                            onclick="deleteCard('${card.id}', event)">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </div>
        `).join('');
    };

    // Инициализация перетаскивания карточек
    const initSortable = () => {
        // Уничтожаем предыдущие экземпляры Sortable
        sortableColumns.forEach(sortable => sortable.destroy());
        sortableColumns = [];

        // Создаем Sortable для каждой колонки
        document.querySelectorAll('.kanban-cards').forEach(column => {
            const sortable = new Sortable(column, {
                group: 'kanban',
                animation: 150,
                ghostClass: 'kanban-card-ghost',
                onEnd: async (evt) => {
                    const cardId = evt.item.dataset.cardId;
                    const newColumnId = evt.to.closest('.kanban-column').dataset.columnId;
                    const newOrder = Array.from(evt.to.children).indexOf(evt.item);

                    try {
                        await fetch(`http://localhost:5281/api/columns/${column.id}/cards/${cardId}/move`, {
                            method: 'PUT',
                            headers: {
                                'Content-Type': 'application/json',
                                'Authorization': `Bearer ${token}`
                            },
                            body: JSON.stringify({
                                newColumnId: newColumnId,
                                newOrder: newOrder + 1
                            })
                        });

                        // Обновляем порядок карточек локально
                        const cards = await (await fetch(`http://localhost:5281/api/columns/${newColumnId}/cards`, {
                            headers: {
                                'Authorization': `Bearer ${token}`
                            }
                        })).json();

                        renderCards(newColumnId, cards);
                    } catch (error) {
                        console.error('Ошибка перемещения карточки:', error);
                        // Возвращаем карточку на место в случае ошибки
                        evt.from.insertBefore(evt.item, evt.from.children[evt.oldIndex]);
                    }
                }
            });

            sortableColumns.push(sortable);
        });
    };

    // Обработчики событий
    // Обработчики событий
    addBoardBtn.addEventListener('click', () => {
        currentEditMode = { type: null, id: null };
        document.getElementById('boardModalTitle').textContent = 'Новая доска';
        document.getElementById('boardName').value = '';
        boardModal.show();
    });

    addColumnBtn.addEventListener('click', () => {
        currentEditMode = { type: null, id: null };
        document.getElementById('columnName').value = '';
        columnModal.show();
    });

    window.showAddCardModal = (columnId) => {
        currentEditMode = { type: null, id: null };
        document.getElementById('cardModalTitle').textContent = 'Новая карточка';
        document.getElementById('cardTitle').value = '';
        document.getElementById('cardDescription').value = '';
        document.getElementById('cardColumnId').value = columnId;
        cardModal.show();
    };
    window.editBoard = (boardId, e) => {
        e.stopPropagation();
        const board = boards.find(b => b.id === boardId);
        currentEditMode = { type: 'board', id: boardId };

        document.getElementById('boardModalTitle').textContent = 'Редактировать доску';
        document.getElementById('boardName').value = board.name;
        boardModal.show();
    };

    // Функции для редактирования и удаления
    window.editColumn = (columnId, e) => {
        e.stopPropagation();
        const column = columns.find(c => c.id === columnId);
        currentEditMode = { type: 'column', id: columnId };

        document.getElementById('columnModalTitle').textContent = 'Редактировать колонку';
        document.getElementById('columnName').value = column.name;
        columnModal.show();
    };

    window.deleteBoard = async (boardId, e) => {
        e.stopPropagation();
        if (!confirm('Вы уверены, что хотите удалить эту доску? Все колонки и карточки также будут удалены.')) return;

        try {
            const response = await fetch(`http://localhost:5281/api/boards/${boardId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Ошибка удаления доски');

            await loadBoards();

            // Если удалили текущую доску, очищаем интерфейс
            if (boardId === currentBoardId) {
                currentBoardId = null;
                currentBoardTitle.textContent = 'Выберите доску';
                addColumnBtn.disabled = true;
                kanbanContainer.innerHTML = `
                    <div class="empty-state text-center py-5">
                        <i class="bi bi-kanban text-muted" style="font-size: 3rem;"></i>
                        <h4 class="mt-3">Выберите доску для начала работы</h4>
                        <p class="text-muted">Или создайте новую доску, нажав на кнопку "+" в сайдбаре</p>
                    </div>
                `;
            }
        } catch (error) {
            console.error('Ошибка:', error);
            alert(error.message);
        }
    };

    window.editColumn = (columnId, e) => {
        e.stopPropagation();
        e.preventDefault();

        const column = columns.find(c => c.id === columnId);
        if (!column) {
            console.error('Колонка не найдена');
            return;
        }

        currentEditMode = { type: 'column', id: columnId };

        // Устанавливаем заголовок и значение поля
        document.getElementById('columnModalTitle').textContent = 'Редактировать колонку';
        document.getElementById('columnName').value = column.name;

        // Показываем модальное окно (оно уже инициализировано в основном коде)
        columnModal.show();
    };

    window.deleteColumn = async (columnId, e) => {
        e.stopPropagation();
        if (!confirm('Вы уверены, что хотите удалить эту колонку? Все карточки также будут удалены.')) return;

        try {
            const response = await fetch(`http://localhost:5281/api/boards/${currentBoardId}/columns/${columnId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Ошибка удаления колонки');

            await loadColumns(currentBoardId);
        } catch (error) {
            console.error('Ошибка:', error);
            alert(error.message);
        }
    };

    window.editCard = (cardId, columnId, e) => {
        e.stopPropagation();
        currentEditMode = { type: 'card', id: cardId };

        const cardElement = document.querySelector(`#cards-${columnId} [data-card-id="${cardId}"]`);
        const title = cardElement.querySelector('.kanban-card-title').textContent;
        const descriptionElement = cardElement.querySelector('.kanban-card-description');
        const description = descriptionElement ? descriptionElement.textContent : '';

        document.getElementById('cardModalTitle').textContent = 'Редактировать карточку';
        document.getElementById('cardTitle').value = title;
        document.getElementById('cardDescription').value = description;
        document.getElementById('cardColumnId').value = columnId;
        cardModal.show();
    };

    window.deleteCard = async (cardId, e) => {
        e.stopPropagation();
        if (!confirm('Вы уверены, что хотите удалить эту карточку?')) return;

        try {
            // Находим колонку, к которой принадлежит карточка
            const column = columns.find(c =>
                document.getElementById(`cards-${c.id}`)?.querySelector(`[data-card-id="${cardId}"]`)
            );

            if (!column) throw new Error('Колонка не найдена');

            const response = await fetch(`http://localhost:5281/api/columns/${column.id}/cards/${cardId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Ошибка удаления карточки');

            await loadColumns(currentBoardId);
        } catch (error) {
            console.error('Ошибка:', error);
            alert(error.message);
        }
    };

    // Поиск досок
    boardSearch.addEventListener('input', (e) => {
        const searchTerm = e.target.value.toLowerCase();
        const boardItems = boardsList.querySelectorAll('.board-item');

        boardItems.forEach(item => {
            const boardName = item.querySelector('.nav-link span').textContent.toLowerCase();
            item.style.display = boardName.includes(searchTerm) ? 'block' : 'none';
        });
    });

    // Переключение сайдбара на мобильных устройствах
    sidebarToggle.addEventListener('click', () => {
        document.getElementById('sidebarMenu').classList.toggle('collapse');
    });

    // Инициализация обработчиков форм
    initFormHandlers();

    // Загрузка начальных данных
    await loadBoards();
});