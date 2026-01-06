document.addEventListener("DOMContentLoaded", function () {

    function renderProducts(data) {
        const productList = document.querySelector(".product-list");
        productList.innerHTML = "";

        if (!data || data.length === 0) {
            productList.innerHTML = `
                <div class="col-12">
                    <p class="text-center">Mehsul tapılmadı</p>
                </div>`;
            return;
        }

        data.forEach(item => {
            productList.innerHTML += `
                <div class="col-md-6 col-lg-4 col-xl-3">
                    <div class="rounded position-relative fruite-item">
                        <div class="fruite-img">
                            <img src="/uploads/${item.image}"
                                 class="img-fluid w-100 rounded-top"
                                 alt="${item.name}">
                        </div>
                        <div class="text-white bg-secondary px-3 py-1 rounded position-absolute"
                             style="top:10px; left:10px">
                            ${item.category}
                        </div>
                        <div class="p-4 border border-secondary border-top-0 rounded-bottom">
                            <h4>${item.name}</h4>
                            <p>${item.description}</p>
                            <div class="d-flex justify-content-between flex-lg-wrap">
                                <p class="text-dark fs-5 fw-bold mb-0">
                                    $${item.price.toFixed(2)} / kg
                                </p>
                                <button class="add-to-basket" data-id="${item.id}">
                                    Add to cart
                                </button>
                            </div>
                        </div>
                    </div>
                </div>`;
        });
    }

    document.querySelector(".search-btn")?.addEventListener("click", function (e) {
        e.preventDefault();
        const searchText = document.querySelector(".input").value;

        fetch(`/Shop/Search?searchText=${searchText}`)
            .then(res => res.json())
            .then(data => renderProducts(data));
    });

    document.querySelectorAll(".category-link").forEach(link => {
        link.addEventListener("click", function (e) {
            e.preventDefault();
            const categoryId = this.dataset.id;

            fetch(`/Shop/Search?categoryId=${categoryId}`)
                .then(res => res.json())
                .then(data => renderProducts(data));
        });
    });

    const sortSelect = document.getElementById("sortSelect");
    sortSelect?.addEventListener("change", function () {
        fetch(`/Shop/Sort?sort=${this.value}`)
            .then(res => res.json())
            .then(data => renderProducts(data));
    });

    const rangeInput = document.getElementById("rangeInput");
    const amount = document.getElementById("amount");

    if (rangeInput && amount) {
        amount.textContent = rangeInput.value;

        rangeInput.addEventListener("input", function () {
            amount.textContent = this.value;

            fetch(`/Shop/FilterByPrice?maxPrice=${this.value}`)
                .then(res => res.json())
                .then(data => renderProducts(data));
        });
    }
});
