(function () {
  const root = document.querySelector("[data-confirm-order]");
  if (!root) {
    return;
  }

  const form = root.querySelector("[data-confirm-form]");
  const subtotalEl = root.querySelector("[data-subtotal]");
  const submitButton = root.querySelector("[data-confirm-submit]");
  const lines = root.querySelectorAll("[data-order-line]");

  function formatMoney(amount) {
    return "£" + amount.toFixed(2);
  }

  function recalculate() {
    let subtotal = 0;

    lines.forEach(function (line) {
      const unitPrice = Number(line.dataset.unitPrice);
      const qtyInput = line.querySelector("[data-qty-input]");
      const lineTotalEl = line.querySelector("[data-line-total]");
      const quantity = Math.max(1, Number(qtyInput.value) || 1);

      qtyInput.value = quantity;
      const lineTotal = unitPrice * quantity;
      lineTotalEl.textContent = formatMoney(lineTotal);
      subtotal += lineTotal;
    });

    subtotalEl.textContent = formatMoney(subtotal);
  }

  lines.forEach(function (line) {
    const qtyInput = line.querySelector("[data-qty-input]");
    qtyInput.addEventListener("input", recalculate);
    qtyInput.addEventListener("change", recalculate);
  });

  if (form && submitButton) {
    form.addEventListener("submit", function () {
      recalculate();
      submitButton.disabled = true;
      submitButton.textContent = "Confirming…";
    });
  }
})();
