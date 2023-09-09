document.addEventListener("DOMContentLoaded", function() {
    const container = document.querySelector(
			'[data-container="tickets"]'
		) as HTMLElement;
    let currentOrder = 'asc';

    async function getTickets(sortBy?: string, order?: string) {
        console.log('gettickets', sortBy, order)
        try {
            // Toggle the order state when making the request
            order = currentOrder;
            currentOrder = currentOrder === 'asc' ? 'desc' : 'asc';
            
            const response = await fetch(
                `/Tickets/SortTickets?sortBy=${sortBy}&order=${order}`
            );

            if (!response.ok) {
                throw new Error(
                    `Error loading partial view: ${response.statusText}`
                );
            }

            const html = await response.text();
            container.innerHTML = '';
            container.innerHTML = html;
        } catch (error) {
            console.error(error);
        }
    }

    getTickets('title');


    container.addEventListener('click', (event) => {
        const header = (event.target as HTMLElement).closest(
                    '[data-sortable]'
                ) as HTMLElement | null;

        if (header) {
            const sortBy = header.getAttribute('data-sort') as string;

            getTickets(sortBy);
        }
    })
});