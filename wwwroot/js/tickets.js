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
document.addEventListener("DOMContentLoaded", function () {
    const container = document.querySelector('[data-container="tickets"]');
    let currentOrder = 'asc';
    function getTickets(sortBy, order) {
        return __awaiter(this, void 0, void 0, function* () {
            console.log('gettickets', sortBy, order);
            try {
                // Toggle the order state when making the request
                order = currentOrder;
                currentOrder = currentOrder === 'asc' ? 'desc' : 'asc';
                const response = yield fetch(`/Tickets/SortTickets?sortBy=${sortBy}&order=${order}`);
                if (!response.ok) {
                    throw new Error(`Error loading partial view: ${response.statusText}`);
                }
                const html = yield response.text();
                container.innerHTML = '';
                container.innerHTML = html;
            }
            catch (error) {
                console.error(error);
            }
        });
    }
    getTickets('title');
    container.addEventListener('click', (event) => {
        const header = event.target.closest('[data-sortable]');
        if (header) {
            const sortBy = header.getAttribute('data-sort');
            getTickets(sortBy);
        }
    });
});
