class Tabs {
    container: HTMLElement;
    tabs: NodeListOf<HTMLButtonElement>;
    contents: NodeListOf<HTMLElement>;

    constructor(container: HTMLElement) {
        this.container = container;
        this.tabs = this.container.querySelectorAll('[data-tab]');
        this.contents = this.container.querySelectorAll('[data-content]');
    }

    init() {
        this.tabs.forEach(tab => {
            tab.addEventListener('click', (e) => {
               this.toggleTabs(e);
               this.toggleContent(e);
            })
        })
    }

    toggleTabs(e: MouseEvent) {
        const clickedTab = e.currentTarget as HTMLButtonElement;

        this.tabs.forEach(tab => {
            tab.classList.remove('active')
            tab.setAttribute('aria-selected', 'false');
        });

        clickedTab.classList.add('active');
        clickedTab.setAttribute('aria-selected', 'true');
    }

    toggleContent(e: Event) {
        const clickedTab = e.currentTarget as HTMLElement;
        const selector = clickedTab.dataset.tab;
        const selectedContent = Array.from(this.contents).find((content: HTMLElement) => content.dataset.content === selector);

        if (selectedContent) {
            this.contents.forEach(content => {
                content.classList.remove('active');
                content.setAttribute('aria-hidden', 'true');
            });

            selectedContent.classList.add('active');
            selectedContent.setAttribute('aria-hidden', 'false');
        }
    }
}