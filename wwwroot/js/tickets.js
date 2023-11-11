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
const ticketsContainer = document.querySelector('[data-container="tickets"]');
let currentTicketsPage = 1;
let currentTicketsLimit = 5;
let currentTicketsSortBy = 'title';
let currentTicketsOrder = 'asc';
function getTickets() {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const url = `/Tickets/SortTickets?page=${currentTicketsPage}&sortBy=${currentTicketsSortBy}&order=${currentTicketsOrder}&limit=${currentTicketsLimit}`;
            const response = yield fetch(url);
            if (!response.ok) {
                throw new Error(`Error loading tickets table: ${response.statusText}`);
            }
            const html = yield response.text();
            ticketsContainer.innerHTML = html;
            const header = ticketsContainer.querySelector(`[data-sort="${currentTicketsSortBy}"]`);
            header.setAttribute('data-order', currentTicketsOrder);
            header.classList.add('active');
            const arrow = header.querySelector('.order');
            arrow === null || arrow === void 0 ? void 0 : arrow.classList.add(currentTicketsOrder);
        }
        catch (error) {
            console.error(error);
        }
    });
}
// load default table order
getTickets();
ticketsContainer.addEventListener('click', (event) => {
    var _a, _b, _c;
    const header = event.target.closest('[data-sortable]');
    if (header) {
        if (header.getAttribute('data-order') === currentTicketsOrder)
            currentTicketsOrder = header.getAttribute('data-order') === 'asc' ? 'desc' : 'asc';
        currentTicketsPage = (_a = parseInt(header.getAttribute('data-page'))) !== null && _a !== void 0 ? _a : currentTicketsPage;
        currentTicketsLimit = (_b = parseInt(header.getAttribute('data-limit'))) !== null && _b !== void 0 ? _b : currentTicketsLimit;
        currentTicketsSortBy = (_c = header.getAttribute('data-sort')) !== null && _c !== void 0 ? _c : currentTicketsSortBy;
        getTickets();
    }
});
