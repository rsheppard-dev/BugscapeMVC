class ListTransfer {
    container: HTMLElement;
    listName: string;
    addButton: HTMLButtonElement;
    removeButton: HTMLButtonElement;
    selection: HTMLSelectElement;
    selected: HTMLSelectElement;
    form: HTMLFormElement;
    submitButton: HTMLButtonElement;

    constructor(container: HTMLElement, listName: string) {
        this.container = container;
        this.listName = listName;
        this.addButton = this.container.querySelector('[data-add-button]') as HTMLButtonElement;
        this.removeButton = this.container.querySelector('[data-remove-button]') as HTMLButtonElement;
        this.selection = this.container.querySelector('[data-selection]') as HTMLSelectElement;
        this.selected = this.container.querySelector('[data-selected]') as HTMLSelectElement;
        this.form = this.container.closest('form') as HTMLFormElement;
        this.submitButton = this.form.querySelector('button[type="submit"]') as HTMLButtonElement;
    }

    init() {
        // load items initially without anything selected
        window.onload = () => {
            Array.from(this.selected.options).forEach(option => {
                option.selected = false;
            });
        };

        // add selected option to the "selected" list
        this.addButton.addEventListener('click', () => this.addOption());
        // remove selected option from the "selected" list
        this.removeButton.addEventListener('click', () => this.removeOption());
        // add hidden inputs to the form for each option in the "selected" list
        this.form.addEventListener('submit', (e: SubmitEvent) => {
            e.preventDefault();
            this.disableSubmitButton();
            this.generateList();
            this.form.submit();
        });
    }

    addOption() {
        Array.from(this.selection.selectedOptions).forEach(option => {
            option.selected = false;
            this.selected.add(option);
        });
    }

    removeOption() {
        Array.from(this.selected.selectedOptions).forEach(option => {
            option.selected = false;
            this.selection.add(option);
        });
    }

    disableSubmitButton() {
        this.submitButton.disabled = true;
        this.submitButton.innerHTML = `
            <div class="flex gap-1 items-center">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg> <span>Updating...</span>
            </div>
        `;
    }

    generateList() {
        // create a hidden input for each option in the "selected" list
        Array.from(this.selected.options).forEach(option => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = this.listName;
            input.value = option.value;
            this.form.appendChild(input);
        });
    }
}