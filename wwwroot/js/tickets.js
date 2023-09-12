"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
const container = document.querySelector('[data-container="tickets"]');
let currentPage = 1;
let currentSortBy = 'title';
let currentOrder = 'asc';
function getTickets() {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const response = yield fetch(`/Tickets/SortTickets?page=${currentPage}&sortBy=${currentSortBy}&order=${currentOrder}`);
            if (!response.ok) {
                throw new Error(`Error loading tickets table: ${response.statusText}`);
            }
            const html = yield response.text();
            container.innerHTML = html;
            const header = container.querySelector(`[data-sort="${currentSortBy}"]`);
            header.setAttribute('data-order', currentOrder);
            header.classList.add('active');
            const arrow = header.querySelector('.order');
            arrow === null || arrow === void 0 ? void 0 : arrow.classList.add(currentOrder);
        }
        catch (error) {
            console.error(error);
        }
    });
}
// load default table order
getTickets();
container.addEventListener('click', (event) => {
    const header = event.target.closest('[data-sortable]');
    if (header) {
        currentPage = parseInt(header.getAttribute('data-page'));
        const order = header.getAttribute('data-order');
        if (order) {
            currentOrder = order === 'asc' ? 'desc' : 'asc';
        }
        currentSortBy = header.getAttribute('data-sort');
        getTickets();
    }
});
