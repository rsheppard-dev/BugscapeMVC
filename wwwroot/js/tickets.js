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
function getTickets(sortBy = 'title') {
    return __awaiter(this, void 0, void 0, function* () {
        let header = container.querySelector(`[data-sort="${sortBy}"]`);
        let order;
        order = header === null || header === void 0 ? void 0 : header.getAttribute('data-order');
        try {
            order = order === 'asc' ? 'desc' : 'asc';
            const response = yield fetch(`/Tickets/SortTickets?sortBy=${sortBy}&order=${order}`);
            if (!response.ok) {
                throw new Error(`Error loading partial view: ${response.statusText}`);
            }
            const html = yield response.text();
            container.innerHTML = html;
            header = container.querySelector(`[data-sort="${sortBy}"]`);
            header.setAttribute('data-order', order);
            header.classList.add('active');
            const arrow = header.querySelector('.order');
            arrow === null || arrow === void 0 ? void 0 : arrow.classList.add(order);
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
        const sortBy = header.getAttribute('data-sort');
        getTickets(sortBy);
    }
});
