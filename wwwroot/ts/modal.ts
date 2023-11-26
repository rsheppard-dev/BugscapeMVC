class Modal {
    modal: HTMLDialogElement;
    openButton: HTMLButtonElement;

    constructor() {
        this.modal = document.querySelector('[data-modal]') as HTMLDialogElement;
        this.openButton = document.querySelector('[data-open-modal]') as HTMLButtonElement;
    }

    init() {
        this.openButton.addEventListener('click', (e: MouseEvent) => {
            e.preventDefault();

            this.modal.showModal();

            // close dialog when clicking outside of it
            this.modal.addEventListener('click', (e: MouseEvent) => {
                console.log(e.target);
                console.log(this.modal)
                if (e.target === this.modal) {
                    this.modal.close();
                }
            });
        });
    }
}
