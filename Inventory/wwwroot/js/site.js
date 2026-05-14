// Generic line-items helper for Purchase / Sale forms
window.LineItems = (function () {
    function recalc(form, includeSales) {
        let total = 0;
        form.querySelectorAll('tbody tr.item-row').forEach(row => {
            const q = parseFloat(row.querySelector('.qty')?.value) || 0;
            const p = parseFloat(row.querySelector('.price')?.value) || 0;
            const lt = q * p;
            row.querySelector('.line-total').textContent = lt.toFixed(2);
            total += lt;
        });
        const t = form.querySelector('.grand-total');
        if (t) t.textContent = total.toFixed(2);
    }

    function init(form, productList, opts) {
        opts = opts || {};
        const tbody = form.querySelector('tbody');
        const addBtn = form.querySelector('.add-row');
        let idx = tbody.querySelectorAll('tr.item-row').length;

        function rowHtml(i) {
            const opts = productList.map(p =>
                `<option value="${p.id}" data-price="${p.salesPrice}" data-unit="${p.unit}">${p.name} (${p.unit})</option>`
            ).join('');
            const salesCol = opts.includeSales ? `<td><input class="sales-price num" name="Items[${i}].SalesPrice" type="number" step="0.01" value="0"/></td>` : '';
            return `<tr class="item-row">
                <td>
                    <select class="product" name="Items[${i}].ProductId" required>
                        <option value="">— Select product —</option>${opts}
                    </select>
                </td>
                <td><input class="qty num" name="Items[${i}].Quantity" type="number" step="0.001" value="1" required/></td>
                <td><input class="price num" name="Items[${i}].${opts.includeSales ? 'PurchasePrice' : 'Price'}" type="number" step="0.01" value="0" required/></td>
                ${salesCol}
                <td class="num line-total">0.00</td>
                <td><button type="button" class="btn btn-danger remove-row">×</button></td>
            </tr>`;
        }

        function addRow() {
            const opts2 = productList.map(p =>
                `<option value="${p.id}" data-price="${p.salesPrice}" data-unit="${p.unit}">${p.name} (${p.unit})</option>`
            ).join('');
            const salesCol = opts.includeSales ? `<td><input class="sales-price num" name="Items[${idx}].SalesPrice" type="number" step="0.01" value="0"/></td>` : '';
            const priceName = opts.includeSales ? 'PurchasePrice' : 'Price';
            const tr = document.createElement('tr');
            tr.className = 'item-row';
            tr.innerHTML = `
                <td>
                    <select class="product" name="Items[${idx}].ProductId" required>
                        <option value="">— Select product —</option>${opts2}
                    </select>
                </td>
                <td><input class="qty num" name="Items[${idx}].Quantity" type="number" step="0.001" value="1" required/></td>
                <td><input class="price num" name="Items[${idx}].${priceName}" type="number" step="0.01" value="0" required/></td>
                ${salesCol}
                <td class="num line-total">0.00</td>
                <td><button type="button" class="btn btn-danger remove-row">×</button></td>`;
            tbody.appendChild(tr);
            idx++;
            wireRow(tr);
        }

        function wireRow(tr) {
            tr.querySelector('.remove-row').addEventListener('click', () => {
                tr.remove(); recalc(form, opts.includeSales);
            });
            const sel = tr.querySelector('.product');
            sel.addEventListener('change', () => {
                const opt = sel.options[sel.selectedIndex];
                const price = parseFloat(opt.dataset.price) || 0;
                tr.querySelector('.price').value = price.toFixed(2);
                if (opts.includeSales) tr.querySelector('.sales-price').value = price.toFixed(2);
                recalc(form, opts.includeSales);
            });
            tr.querySelectorAll('.qty,.price,.sales-price').forEach(i =>
                i.addEventListener('input', () => recalc(form, opts.includeSales)));
        }

        addBtn.addEventListener('click', addRow);
        tbody.querySelectorAll('tr.item-row').forEach(wireRow);
        if (tbody.children.length === 0) addRow();
        recalc(form, opts.includeSales);
    }

    return { init };
})();
