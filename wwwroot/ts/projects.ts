const projectsContainer = document.querySelector(
	'[data-container="projects"]'
) as HTMLElement;

let currentProjectsPage = 1;
let currentProjectsLimit = 5;
let currentProjectsSortBy = 'enddate';
let currentProjectsOrder = 'desc';

async function getProjects() {
	try {
		const url = `/Projects/SortProjects?page=${currentProjectsPage}&sortBy=${currentProjectsSortBy}&order=${currentProjectsOrder}&limit=${currentProjectsLimit}`;
		const response = await fetch(url);

		if (!response.ok) {
			throw new Error(`Error loading projects table: ${response.statusText}`);
		}

		const html = await response.text();
		projectsContainer.innerHTML = html;

		const header = projectsContainer.querySelector(`[data-sort="${currentProjectsSortBy}"]`)!;
		header.setAttribute('data-order', currentProjectsOrder);

		header.classList.add('active');

		const arrow = header.querySelector('.order');

		arrow?.classList.add(currentProjectsOrder);
	} catch (error) {
		console.error(error);
	}
}

// load default table order
getProjects();

projectsContainer.addEventListener('click', event => {
	const header = (event.target as HTMLElement).closest('[data-sortable]');

	if (header) {
		if (header.getAttribute('data-order') === currentProjectsOrder)
			currentProjectsOrder =
				header.getAttribute('data-order') === 'asc' ? 'desc' : 'asc';

		currentProjectsPage =
			parseInt(header.getAttribute('data-page') as string) ?? currentProjectsPage;
		currentProjectsSortBy = header.getAttribute('data-sort') ?? currentProjectsSortBy;
		currentProjectsLimit = parseInt(header.getAttribute('data-limit') as string) ?? currentProjectsLimit;

		getProjects();
	}
});
