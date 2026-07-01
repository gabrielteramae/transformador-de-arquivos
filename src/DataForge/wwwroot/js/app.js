const MAX_FILE_SIZE = 5 * 1024 * 1024;
let selectedFile = null;
let outputFormat = 'json';

document.getElementById('fileInput').addEventListener('change', e => {
    const f = e.target.files[0];
    if (!f) return;
    if (f.size > MAX_FILE_SIZE) {
        alert('Arquivo muito grande. O limite é 5 MB.');
        e.target.value = '';
        return;
    }
    selectedFile = f;
    const el = document.getElementById('fileName');
    el.textContent = f.name;
    el.style.display = 'block';
});

const dropZone = document.getElementById('dropZone');
dropZone.addEventListener('dragover', e => { e.preventDefault(); dropZone.style.background = '#f5f5f3'; });
dropZone.addEventListener('dragleave', () => { dropZone.style.background = ''; });
dropZone.addEventListener('drop', e => {
    e.preventDefault();
    dropZone.style.background = '';
    const f = e.dataTransfer.files[0];
    if (!f) return;
    if (f.size > MAX_FILE_SIZE) {
        alert('Arquivo muito grande. O limite é 5 MB.');
        return;
    }
    selectedFile = f;
    const el = document.getElementById('fileName');
    el.textContent = f.name;
    el.style.display = 'block';
});

document.querySelectorAll('#fmt button').forEach(btn => {
    btn.addEventListener('click', () => {
        document.querySelectorAll('#fmt button').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        outputFormat = btn.dataset.val;
    });
});

document.addEventListener('keydown', e => {
    if (e.key === 'Escape') fecharAjuda();
});

function abrirAjuda() {
    document.getElementById('modalOverlay').classList.add('show');
}

function fecharAjuda() {
    document.getElementById('modalOverlay').classList.remove('show');
}

function validateFilter(value) {
    if (!value) return true;
    return /^[a-zA-Z0-9_]+\s*(=|>|<|>=|<=)\s*.+$/.test(value.trim());
}

document.getElementById('filter').addEventListener('blur', () => {
    const val = document.getElementById('filter').value.trim();
    const input = document.getElementById('filter');
    const errMsg = document.getElementById('filterError');
    if (val && !validateFilter(val)) {
        input.classList.add('input-error');
        errMsg.classList.add('show');
    } else {
        input.classList.remove('input-error');
        errMsg.classList.remove('show');
    }
});

document.getElementById('filter').addEventListener('input', () => {
    const input = document.getElementById('filter');
    const errMsg = document.getElementById('filterError');
    input.classList.remove('input-error');
    errMsg.classList.remove('show');
});

async function transformar() {
    if (!selectedFile) { alert('Selecione um arquivo primeiro.'); return; }

    const filterVal = document.getElementById('filter').value.trim();
    if (filterVal && !validateFilter(filterVal)) {
        document.getElementById('filter').classList.add('input-error');
        document.getElementById('filterError').classList.add('show');
        document.getElementById('filter').focus();
        return;
    }

    const form = new FormData();
    form.append('file', selectedFile);
    const cols = document.getElementById('columns').value.trim();
    const rename = document.getElementById('rename').value.trim();
    if (filterVal) form.append('filter', filterVal);
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
        const downloadBtn = document.getElementById('downloadBtn');
        resultEl.classList.add('show');
        if (data.success) {
            badge.textContent = 'ok';
            badge.className = 'badge';
            meta.textContent = data.rowCount + ' linha' + (data.rowCount !== 1 ? 's' : '');
            output.textContent = data.data;
            downloadBtn.style.display = 'inline-flex';
        } else {
            badge.textContent = 'erro';
            badge.className = 'badge err';
            meta.textContent = '';
            output.textContent = data.error || 'Erro desconhecido.';
            downloadBtn.style.display = 'none';
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
    if (!txt) return;
    navigator.clipboard.writeText(txt).then(() => {
        const btn = document.getElementById('copyBtn');
        btn.textContent = 'copiado';
        setTimeout(() => {
            btn.innerHTML = '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="9" y="9" width="13" height="13" rx="2"/><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/></svg> copiar';
        }, 1500);
    });
}

function baixar() {
    const txt = document.getElementById('output').textContent;
    if (!txt) return;
    const mimeTypes = { json: 'application/json', csv: 'text/csv', xml: 'application/xml' };
    const blob = new Blob([txt], { type: mimeTypes[outputFormat] });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'resultado.' + outputFormat;
    a.click();
    URL.revokeObjectURL(url);
}