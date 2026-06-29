let selectedFile = null;
let outputFormat = 'json';

document.getElementById('fileInput').addEventListener('change', e => {
    selectedFile = e.target.files[0];
    if (selectedFile) {
        const el = document.getElementById('fileName');
        el.textContent = selectedFile.name;
        el.style.display = 'block';
    }
});

const dropZone = document.getElementById('dropZone');
dropZone.addEventListener('dragover', e => { e.preventDefault(); dropZone.style.background = '#f5f5f3'; });
dropZone.addEventListener('dragleave', () => { dropZone.style.background = ''; });
dropZone.addEventListener('drop', e => {
    e.preventDefault();
    dropZone.style.background = '';
    const f = e.dataTransfer.files[0];
    if (f) {
        selectedFile = f;
        const el = document.getElementById('fileName');
        el.textContent = f.name;
        el.style.display = 'block';
    }
});

document.querySelectorAll('#fmt button').forEach(btn => {
    btn.addEventListener('click', () => {
        document.querySelectorAll('#fmt button').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        outputFormat = btn.dataset.val;
    });
});

async function transformar() {
    if (!selectedFile) { alert('Selecione um arquivo primeiro.'); return; }
    const form = new FormData();
    form.append('file', selectedFile);
    const filter = document.getElementById('filter').value.trim();
    const cols = document.getElementById('columns').value.trim();
    const rename = document.getElementById('rename').value.trim();
    if (filter) form.append('filter', filter);
    if (cols) form.append('selectColumns', cols);
    if (rename) form.append('renameColumns', rename);
    form.append('outputFormat', outputFormat);
    const btn = document.querySelector('.btn-run');
    btn.textContent = 'Processando…';
    btn.disabled = true;
    try {
        const res = await fetch('/api/Transform', { method: 'POST', body: form });
        const data = await res.json();
        const resultEl = document.getElementById('result');
        const badge = document.getElementById('statusBadge');
        const meta = document.getElementById('metaInfo');
        const output = document.getElementById('output');
        resultEl.classList.add('show');
        if (data.success) {
            badge.textContent = 'ok';
            badge.className = 'badge';
            meta.textContent = data.rowCount + ' linha' + (data.rowCount !== 1 ? 's' : '');
            output.textContent = data.data;
        } else {
            badge.textContent = 'erro';
            badge.className = 'badge err';
            meta.textContent = '';
            output.textContent = data.error || 'Erro desconhecido.';
        }
    } catch (e) {
        alert('Não foi possível conectar à API.');
    } finally {
        btn.textContent = 'Transformar';
        btn.disabled = false;
    }
}

function copiar() {
    const txt = document.getElementById('output').textContent;
    navigator.clipboard.writeText(txt).then(() => {
        const btn = document.querySelector('.copy-btn');
        btn.textContent = 'copiado';
        setTimeout(() => {
            btn.innerHTML = '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="9" y="9" width="13" height="13" rx="2"/><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/></svg> copiar';
        }, 1500);
    });
}