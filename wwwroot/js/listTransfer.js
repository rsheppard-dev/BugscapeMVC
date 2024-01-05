"use strict";
var ListTransfer = /** @class */ (function () {
    function ListTransfer(container, listName) {
        this.container = container;
        this.listName = listName;
        this.addButton = this.container.querySelector('[data-add-button]');
        this.removeButton = this.container.querySelector('[data-remove-button]');
        this.selection = this.container.querySelector('[data-selection]');
        this.selected = this.container.querySelector('[data-selected]');
        this.form = this.container.closest('form');
        this.submitButton = this.form.querySelector('button[type="submit"]');
    }
    ListTransfer.prototype.init = function () {
        var _this = this;
        // load items initially without anything selected
        window.onload = function () {
            Array.from(_this.selected.options).forEach(function (option) {
                option.selected = false;
            });
        };
        // add selected option to the "selected" list
        this.addButton.addEventListener('click', function () { return _this.addOption(); });
        // remove selected option from the "selected" list
        this.removeButton.addEventListener('click', function () { return _this.removeOption(); });
        // add hidden inputs to the form for each option in the "selected" list
        this.form.addEventListener('submit', function (e) {
            e.preventDefault();
            _this.disableSubmitButton();
            _this.generateList();
            _this.form.submit();
        });
    };
    ListTransfer.prototype.addOption = function () {
        var _this = this;
        Array.from(this.selection.selectedOptions).forEach(function (option) {
            option.selected = false;
            _this.selected.add(option);
        });
    };
    ListTransfer.prototype.removeOption = function () {
        var _this = this;
        Array.from(this.selected.selectedOptions).forEach(function (option) {
            option.selected = false;
            _this.selection.add(option);
        });
    };
    ListTransfer.prototype.disableSubmitButton = function () {
        this.submitButton.disabled = true;
        this.submitButton.innerHTML = "\n            <div class=\"flex gap-1 items-center\">\n                <svg xmlns=\"http://www.w3.org/2000/svg\" class=\"h-4 w-4 animate-spin\" fill=\"none\" viewBox=\"0 0 24 24\" stroke=\"currentColor\" stroke-width=\"2\">\n                    <path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15\" />\n                </svg> <span>Updating...</span>\n            </div>\n        ";
    };
    ListTransfer.prototype.generateList = function () {
        var _this = this;
        // create a hidden input for each option in the "selected" list
        Array.from(this.selected.options).forEach(function (option) {
            var input = document.createElement('input');
            input.type = 'hidden';
            input.name = _this.listName;
            input.value = option.value;
            _this.form.appendChild(input);
        });
    };
    return ListTransfer;
}());
