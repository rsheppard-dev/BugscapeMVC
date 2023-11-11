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
const projectsContainer = document.querySelector('[data-container="projects"]');
let currentProjectsPage = 1;
let currentProjectsLimit = 10;
let currentProjectsSortBy = 'enddate';
let currentProjectsOrder = 'desc';
function getProjects() {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const url = `/Projects/SortProjects?page=${currentProjectsPage}&sortBy=${currentProjectsSortBy}&order=${currentProjectsOrder}&limit=${currentProjectsLimit}`;
            const response = yield fetch(url);
            if (!response.ok) {
                throw new Error(`Error loading projects table: ${response.statusText}`);
            }
            const html = yield response.text();
            projectsContainer.innerHTML = html;
            const header = projectsContainer.querySelector(`[data-sort="${currentProjectsSortBy}"]`);
            header.setAttribute('data-order', currentProjectsOrder);
            header.classList.add('active');
            const arrow = header.querySelector('.order');
            arrow === null || arrow === void 0 ? void 0 : arrow.classList.add(currentProjectsOrder);
        }
        catch (error) {
            console.error(error);
        }
    });
}
// load default table order
getProjects();
projectsContainer.addEventListener('click', (event) => {
    var _a, _b, _c;
    const header = event.target.closest('[data-sortable]');
    if (header) {
        if (header.getAttribute('data-order') === currentProjectsOrder)
            currentProjectsOrder =
                header.getAttribute('data-order') === 'asc' ? 'desc' : 'asc';
        currentProjectsPage =
            (_a = parseInt(header.getAttribute('data-page'))) !== null && _a !== void 0 ? _a : currentProjectsPage;
        currentProjectsSortBy =
            (_b = header.getAttribute('data-sort')) !== null && _b !== void 0 ? _b : currentProjectsSortBy;
        currentProjectsLimit =
            (_c = parseInt(header.getAttribute('data-limit'))) !== null && _c !== void 0 ? _c : currentProjectsLimit;
        getProjects();
    }
});
