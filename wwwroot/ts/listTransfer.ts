class ListTransfer {
    container: HTMLElement;
    listName: string;
    addButton: HTMLButtonElement;
    removeButton: HTMLButtonElement;
    selection: HTMLSelectElement;
    selected: HTMLSelectElement;
    form: HTMLFormElement;

    constructor(container: HTMLElement, listName: string) {
        this.container = container;
        this.listName = listName;
        this.addButton = this.container.querySelector('[data-add-button]') as HTMLButtonElement;
        this.removeButton = this.container.querySelector('[data-remove-button]') as HTMLButtonElement;
        this.selection = this.container.querySelector('[data-selection]') as HTMLSelectElement;
        this.selected = this.container.querySelector('[data-selected]') as HTMLSelectElement;
        this.form = this.container.closest('form') as HTMLFormElement;
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
        this.form.addEventListener('submit', (e: SubmitEvent) => this.generateList());
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