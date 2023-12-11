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
        this.form.addEventListener('submit', function (e) { return _this.generateList(); });
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
