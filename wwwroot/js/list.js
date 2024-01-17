"use strict";
var List = /** @class */ (function () {
    function List(container, listName) {
        this.container = container;
        this.listName = listName;
        this.addButton = this.container.querySelector('[data-add-button]');
        this.removeButton = this.container.querySelectorAll('[data-remove-button]');
        this.listItems = this.container.querySelector('[data-list-items]');
        this.selection = this.container.querySelector('[data-selection]');
        this.form = this.container.closest('form');
        this.submitButton = this.form.querySelector('button[type="submit"]');
    }
    List.prototype.init = function () {
        var _this = this;
        // if users clicks add button, add selected option to the "selected" list and remove from the "selection" list
        this.addButton.addEventListener('click', function () { return _this.add(); });
        // if users clicks remove button, remove selected option from the "selected" list and back to the "selection" list
        this.removeButton.forEach(function (button) {
            button.addEventListener('click', function (e) { return _this.remove(e); });
        });
        // on submit, add hidden inputs to the form for each option in the "selected" list
        this.form.addEventListener('submit', function (e) {
            e.preventDefault();
            _this.disableSubmitButton();
            _this.generateList();
            _this.form.submit();
        });
        // if there are no options in the "selection" list, disable the add option
        if (this.selection.options.length <= 1) {
            this.addButton.disabled = true;
            this.selection.disabled = true;
        }
        else {
            this.addButton.disabled = false;
            this.selection.disabled = false;
        }
    };
    List.prototype.add = function () {
        var _this = this;
        // get the selected option from the selection list
        var selectedOption = this.selection.options[this.selection.selectedIndex];
        if (selectedOption.value === "")
            return;
        // get the template
        var template = document.querySelector('[data-template]');
        // clone the template
        var clone = document.importNode(template.content, true);
        // set the value and text of the new content
        var li = clone.querySelector('li');
        li.setAttribute('data-value', selectedOption.value);
        li.setAttribute('data-text', selectedOption.text);
        // add text to the span
        var spanElement = li.querySelector('span');
        if (spanElement)
            spanElement.innerHTML = selectedOption.text;
        // add data to the button
        var button = li.querySelector('button');
        button.title = "Remove ".concat(selectedOption.text);
        if (button)
            button.addEventListener('click', function (e) { return _this.remove(e); });
        // add the new content to the list
        this.listItems.appendChild(clone);
        // remove the selected option from the selection list
        this.selection.remove(this.selection.selectedIndex);
        // if there are no options in the "selection" list, disable the add option
        if (this.selection.options.length <= 1) {
            this.addButton.disabled = true;
            this.selection.disabled = true;
        }
        else {
            this.addButton.disabled = false;
            this.selection.disabled = false;
        }
    };
    List.prototype.remove = function (e) {
        // get the button that was clicked
        var button = e.currentTarget;
        // get the parent li element
        var li = button.parentElement;
        // get the value and text of the selected option
        var value = li.getAttribute('data-value');
        var text = li.getAttribute('data-text');
        // remove the li element from the list if it exists
        if (li)
            li.remove();
        // add the option back to the selection list
        var option = document.createElement('option');
        option.value = value !== null && value !== void 0 ? value : '';
        option.text = text !== null && text !== void 0 ? text : '';
        this.selection.add(option);
        // if there are no options in the "selection" list, disable the add option
        if (this.selection.options.length <= 1) {
            this.addButton.disabled = true;
            this.selection.disabled = true;
        }
        else {
            this.addButton.disabled = false;
            this.selection.disabled = false;
        }
    };
    List.prototype.disableSubmitButton = function () {
        this.submitButton.disabled = true;
        this.submitButton.innerHTML = "\n            <div class=\"flex gap-1 items-center\">\n                <svg xmlns=\"http://www.w3.org/2000/svg\" class=\"h-4 w-4 animate-spin\" fill=\"none\" viewBox=\"0 0 24 24\" stroke=\"currentColor\" stroke-width=\"2\">\n                    <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15\" />\n                </svg> <span>Updating...</span>\n            </div>\n        ";
    };
    List.prototype.generateList = function () {
        var _this = this;
        // create a hidden input for each option in the "selected" list
        Array.from(this.listItems.children).forEach(function (li) {
            var _a;
            var input = document.createElement('input');
            input.type = 'hidden';
            input.name = _this.listName;
            input.value = (_a = li.getAttribute('data-value')) !== null && _a !== void 0 ? _a : '';
            _this.form.appendChild(input);
        });
    };
    return List;
}());
