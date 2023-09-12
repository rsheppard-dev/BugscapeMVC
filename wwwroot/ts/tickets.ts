const container = document.querySelector(
        '[data-container="tickets"]'
    ) as HTMLElement;

let currentPage = 1;
let currentSortBy = 'title';
let currentOrder = 'asc';

async function getTickets() {
    try {
        const response = await fetch(`/Tickets/SortTickets?page=${currentPage}&sortBy=${currentSortBy}&order=${currentOrder}`);

        if (!response.ok) {
            throw new Error(
                `Error loading tickets table: ${response.statusText}`
            );
        }

        const html = await response.text();
        container.innerHTML = html;

        const header = container.querySelector(`[data-sort="${currentSortBy}"]`)!;
        header.setAttribute('data-order', currentOrder);

        header.classList.add('active')
    
        const arrow = header.querySelector('.order');

        arrow?.classList.add(currentOrder);

    } catch (error) {
        console.error(error);
    }
}

// load default table order
getTickets();

container.addEventListener('click', (event) => {
    const header = (event.target as HTMLElement).closest(
                '[data-sortable]'
            );

    if (header) {
        currentPage = parseInt(header.getAttribute('data-page') as string);

        const order = header.getAttribute('data-order');

		if (order)
        {
            currentOrder = order  === 'asc' ? 'desc' : 'asc';
        }

        currentSortBy = header.getAttribute('data-sort') as string;

        getTickets();
    }
});