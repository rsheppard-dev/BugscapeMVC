const container = document.querySelector(
        '[data-container="tickets"]'
    ) as HTMLElement;

async function getTickets(sortBy = 'title') {
    let header = container.querySelector(`[data-sort="${sortBy}"]`);
    let order: string;

    order = header?.getAttribute('data-order') as string;

    try {
        order = order === 'asc' ? 'desc' : 'asc';
        
        const response = await fetch(
            `/Tickets/SortTickets?sortBy=${sortBy}&order=${order}`
        );

        if (!response.ok) {
            throw new Error(
                `Error loading partial view: ${response.statusText}`
            );
        }

        const html = await response.text();
        container.innerHTML = html;

        header = container.querySelector(`[data-sort="${sortBy}"]`)!;
        header.setAttribute('data-order', order);

        header.classList.add('active')
    

        const arrow = header.querySelector('.order');

        arrow?.classList.add(order);

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
        const sortBy = header.getAttribute('data-sort') as string;

        getTickets(sortBy);
    }
});