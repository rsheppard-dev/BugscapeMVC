class Dropdown {
    container: HTMLElement;
    trigger: HTMLElement;
    content: HTMLElement;

    constructor(container: HTMLElement) {
        this.container = container;
        this.trigger = container.querySelector(".trigger")!;
        this.content = container.querySelector(".content")!;
    }

    init() {
        this.trigger.addEventListener('click', () => {
            this.trigger.classList.toggle('active');
            this.content.classList.toggle('active');
        })
    }
}

// create dropdowns
const dropdowns = Array.from(document.querySelectorAll(".dropdown")) as HTMLElement[];

dropdowns.forEach(dropdown => {
    const instance = new Dropdown(dropdown);

    instance.init();
});