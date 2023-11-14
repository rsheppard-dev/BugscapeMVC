"use strict";
var Dropdown = /** @class */ (function () {
    function Dropdown(container) {
        this.container = container;
        this.trigger = container.querySelector(".trigger");
        this.content = container.querySelector(".content");
    }
    Dropdown.prototype.init = function () {
        var _this = this;
        this.trigger.addEventListener('click', function () {
            _this.trigger.classList.toggle('active');
            _this.content.classList.toggle('active');
        });
    };
    return Dropdown;
}());
// create dropdowns
var dropdowns = Array.from(document.querySelectorAll(".dropdown"));
dropdowns.forEach(function (dropdown) {
    var instance = new Dropdown(dropdown);
    instance.init();
});
