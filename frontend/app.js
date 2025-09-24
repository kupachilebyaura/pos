const apiCandidates = [
  'http://localhost:5000',
  'https://localhost:5001',
  'http://localhost:5002',
  'https://localhost:5003'
];

let apiBase = window.apiBase || null;

async function probeApi() {
  if (apiBase) return apiBase;
  for (const base of apiCandidates) {
    try {
      const r = await fetch(`${base}/api/cash/ping`, { method: 'GET', mode: 'cors' });
      if (r.ok) { apiBase = base; return base; }
    } catch (e) {
      // ignore
    }
  }
  throw new Error('No API candidate responded');
}

async function fetchConsolidated(from, to) {
  const base = await probeApi();
  const url = `${base}/api/cash/consolidated?from=${from}&to=${to}`;
  const r = await fetch(url, { mode: 'cors' });
  if (!r.ok) throw new Error(`HTTP ${r.status}`);
  return r.json();
}

function renderTable(data) {
  const tbody = document.querySelector('#tbl tbody');
  tbody.innerHTML = '';
  data.forEach(row => {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${row.Group}</td>
      <td>${row.CantidadArqueos}</td>
      <td>${row.EfectivoInicial.toFixed(2)}</td>
      <td>${row.VentasEfectivo.toFixed(2)}</td>
      <td>${row.Ingresos.toFixed(2)}</td>
      <td>${row.Retiros.toFixed(2)}</td>
      <td>${row.MontoFinal.toFixed(2)}</td>
      <td>${row.Diferencia.toFixed(2)}</td>
    `;
    tbody.appendChild(tr);
  });
}

document.getElementById('load').addEventListener('click', async () => {
  const from = document.getElementById('from').value;
  const to = document.getElementById('to').value;
  try {
    const data = await fetchConsolidated(from, to);
    renderTable(data);
  } catch (err) {
    document.getElementById('debug').style.display = 'block';
    document.getElementById('debug').textContent = err.toString();
  }
});

// Auto-load on page open
window.addEventListener('load', () => document.getElementById('load').click());
