class List {
    container: HTMLElement;
    listName: string;
    addButton: HTMLButtonElement;
    removeButton: NodeListOf<HTMLButtonElement>;
    listItems: HTMLUListElement;
    selection: HTMLSelectElement;
    form: HTMLFormElement;
    submitButton: HTMLButtonElement;

    constructor(container: HTMLElement, listName: string) {
        this.container = container;
        this.listName = listName;
        this.addButton = this.container.querySelector('[data-add-button]') as HTMLButtonElement;
        this.removeButton = this.container.querySelectorAll('[data-remove-button]') as NodeListOf<HTMLButtonElement>;
        this.listItems = this.container.querySelector('[data-list-items]') as HTMLUListElement;
        this.selection = this.container.querySelector('[data-selection]') as HTMLSelectElement;
        this.form = this.container.closest('form') as HTMLFormElement;
        this.submitButton = this.form.querySelector('button[type="submit"]') as HTMLButtonElement;
    }

    init() {
        // if users clicks add button, add selected option to the "selected" list and remove from the "selection" list
        this.addButton.addEventListener('click', () => this.add());

        // if users clicks remove button, remove selected option from the "selected" list and back to the "selection" list
        this.removeButton.forEach(button => {
            button.addEventListener('click', (e) => this.remove(e));
        });

        // on submit, add hidden inputs to the form for each option in the "selected" list
        this.form.addEventListener('submit', (e: SubmitEvent) => {
            e.preventDefault();
            this.disableSubmitButton();
            this.generateList();
            this.form.submit();
        });

        // if there are no options in the "selection" list, disable the add option
        if (this.selection.options.length <= 1) {
            this.addButton.disabled = true;
            this.selection.disabled = true;
        } else {
            this.addButton.disabled = false;
            this.selection.disabled = false;
        }
    }

    add() {
        // get the selected option from the selection list
        const selectedOption = this.selection.options[this.selection.selectedIndex];

        if (selectedOption.value === "") return;

        // get the template
        const template = document.querySelector('[data-template]') as HTMLTemplateElement;

        // clone the template
        const clone = document.importNode(template.content, true);

        // set the value and text of the new content
        const li = clone.querySelector('li') as HTMLLIElement;
        li.setAttribute('data-value', selectedOption.value);
        li.setAttribute('data-text', selectedOption.text);

        // add text to the span
        const spanElement = li.querySelector('span');
        if (spanElement) spanElement.innerHTML = selectedOption.text;

        // add data to the button
        const button = li.querySelector('button') as HTMLButtonElement;
        button.title = `Remove ${selectedOption.text}`;
        if (button) button.addEventListener('click', (e) => this.remove(e));

        // add the new content to the list
        this.listItems.appendChild(clone);

        // remove the selected option from the selection list
        this.selection.remove(this.selection.selectedIndex);

       // if there are no options in the "selection" list, disable the add option
       if (this.selection.options.length <= 1) {
            this.addButton.disabled = true;
            this.selection.disabled = true;
        } else {
            this.addButton.disabled = false;
            this.selection.disabled = false;
        }
    }

    remove(e: MouseEvent) {
        // get the button that was clicked
        const button = e.currentTarget as HTMLButtonElement;

        // get the parent li element
        const li = button.parentElement as HTMLLIElement;

        // get the value and text of the selected option
        const value = li.getAttribute('data-value');
        const text = li.getAttribute('data-text');

        // remove the li element from the list if it exists
        if (li) li.remove();

        // add the option back to the selection list
        const option = document.createElement('option') as HTMLOptionElement;
        option.value = value ?? '';
        option.text = text ?? '';
        this.selection.add(option);

        // if there are no options in the "selection" list, disable the add option
        if (this.selection.options.length <= 1) {
            this.addButton.disabled = true;
            this.selection.disabled = true;
        } else {
            this.addButton.disabled = false;
            this.selection.disabled = false;
        }
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
        Array.from(this.listItems.children).forEach(li => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = this.listName;
            input.value = li.getAttribute('data-value') ?? '';
            this.form.appendChild(input);
        });
    }
}