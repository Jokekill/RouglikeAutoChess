const symbols = {
    'p': '♟', 'r': '♜', 'n': '♞', 'b': '♝', 'q': '♛', 'k': '♚',
    'P': '♙', 'R': '♖', 'N': '♘', 'B': '♗', 'Q': '♕', 'K': '♔'
};

function createBoard() {
    const table = document.getElementById('board');
    for (let r = 0; r < 8; r++) {
        const row = document.createElement('tr');
        for (let c = 0; c < 8; c++) {
            const cell = document.createElement('td');
            cell.dataset.row = r;
            cell.dataset.col = c;
            cell.className = (r + c) % 2 === 0 ? 'white' : 'black';
            row.appendChild(cell);
        }
        table.appendChild(row);
    }
}

function parseFEN(fen) {
    const rows = fen.split(' ')[0].split('/');
    const board = [];
    for (let r = 0; r < 8; r++) {
        const row = [];
        for (const ch of rows[r]) {
            if (!isNaN(ch)) {
                for (let i = 0; i < Number(ch); i++) row.push(' ');
            } else {
                row.push(ch);
            }
        }
        board.push(row);
    }
    return board;
}

function renderBoard(arr) {
    for (let r = 0; r < 8; r++) {
        for (let c = 0; c < 8; c++) {
            const cell = document.querySelector(`td[data-row='${r}'][data-col='${c}']`);
            const piece = arr[r][c];
            cell.textContent = symbols[piece] || '';
        }
    }
}

async function loadRandom() {
    const res = await fetch('/board/random');
    const fen = await res.text();
    renderBoard(parseFEN(fen));
}

async function simulate() {
    const res = await fetch('/move', { method: 'POST' });
    const fen = await res.text();
    renderBoard(parseFEN(fen));
}

document.addEventListener('DOMContentLoaded', () => {
    createBoard();
    loadRandom();
    document.getElementById('simulateBtn').addEventListener('click', simulate);
});
