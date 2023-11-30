class Modal {
    modal: HTMLDialogElement;
    openButton: HTMLButtonElement;
    closeButton: HTMLButtonElement;

    constructor() {
        this.modal = document.querySelector('[data-modal]') as HTMLDialogElement;
        this.openButton = document.querySelector('[data-open-modal]') as HTMLButtonElement;
        this.closeButton = document.querySelector('[data-close-modal]') as HTMLButtonElement;
    }

    init() {
        // open dialog when clicking open button
        this.openButton.addEventListener('click', () => {
            this.modal.showModal();

            // close dialog when clicking close button
            this.closeButton.addEventListener('click', () => {
                this.modal.close();
            });

            // close dialog when clicking outside of it
            this.modal.addEventListener('click', (e: MouseEvent) => {
                if (e.target === this.modal) {
                    this.modal.close();
                }
            });
        });
    }
}
