const ticketsContainer = document.querySelector(
        '[data-container="tickets"]'
    ) as HTMLElement;

let currentTicketsPage = 1;
let currentTicketsLimit = 5;
let currentTicketsSortBy = 'title';
let currentTicketsOrder = 'asc';

async function getTickets() {
    try {
        const url = `/Tickets/SortTickets?page=${currentTicketsPage}&sortBy=${currentTicketsSortBy}&order=${currentTicketsOrder}&limit=${currentTicketsLimit}`;
        const response = await fetch(url);

        if (!response.ok) {
            throw new Error(
                `Error loading tickets table: ${response.statusText}`
            );
        }

        const html = await response.text();
        ticketsContainer.innerHTML = html;

        const header = ticketsContainer.querySelector(`[data-sort="${currentTicketsSortBy}"]`)!;
        header.setAttribute('data-order', currentTicketsOrder);

        header.classList.add('active')
    
        const arrow = header.querySelector('.order');

        arrow?.classList.add(currentTicketsOrder);

    } catch (error) {
        console.error(error);
    }
}

// load default table order
getTickets();

ticketsContainer.addEventListener('click', (event) => {
    const header = (event.target as HTMLElement).closest(
                '[data-sortable]'
            );

    if (header) {
        if (header.getAttribute('data-order') === currentTicketsOrder)
            currentTicketsOrder = header.getAttribute('data-order') === 'asc' ? 'desc' : 'asc';
    
        currentTicketsPage = parseInt(header.getAttribute('data-page') as string) ?? currentTicketsPage;
        currentTicketsLimit = parseInt(header.getAttribute('data-limit') as string) ?? currentTicketsLimit;
        currentTicketsSortBy = header.getAttribute('data-sort') ?? currentTicketsSortBy;

        getTickets();
    }
});