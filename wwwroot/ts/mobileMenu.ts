class MobileMenu {
    toggleButton: HTMLButtonElement;
    menu: HTMLElement;
    isOpen: boolean;

    constructor() {
        this.toggleButton = document.querySelector('[data-toggle-menu]') as HTMLButtonElement;
        this.menu = document.querySelector('[data-menu]') as HTMLElement;
        this.isOpen = false;
    }

    init() {
        this.toggleButton.addEventListener('click', (event) => {
            event.stopPropagation();

            if (this.isOpen) {
                this.closeMenu();
            } else {
                this.openMenu();
            }
        });

        document.addEventListener('click', (event) => {
            if (!this.menu.contains(event.target as Node) && this.isOpen) {
                this.closeMenu();
            }
        });
    }


    openMenu() {
        this.isOpen = true;
        this.menu.classList.remove('-translate-x-full');
        this.menu.classList.add('translate-x-0');
        this.toggleButton.innerHTML = `
        <svg xmlns="http://www.w3.org/2000/svg" aria-hidden fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-6 h-6 stroke-dark">
            <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
        </svg>
        `
        this.toggleButton.title = 'Close Menu';
    }

    closeMenu() {
        this.isOpen = false;
        this.menu.classList.remove('translate-x-0');
        this.menu.classList.add('-translate-x-full');
        this.toggleButton.innerHTML = `
        <svg xmlns="http://www.w3.org/2000/svg" aria-hidden fill="none" viewBox="0 0 24 24" stroke-width="1.5"
            stroke="currentColor" class="w-6 h-6 stroke-dark">
            <path stroke-linecap="round" stroke-linejoin="round"
                d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5" />
        </svg>
        `
        this.toggleButton.title = 'Open Menu';
    }
}

const instance = new MobileMenu();

instance.init();