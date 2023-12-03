"use strict";
var Tabs = /** @class */ (function () {
    function Tabs(container) {
        this.container = container;
        this.tabs = this.container.querySelectorAll('[data-tab]');
        this.contents = this.container.querySelectorAll('[data-content]');
    }
    Tabs.prototype.init = function () {
        var _this = this;
        this.tabs.forEach(function (tab) {
            tab.addEventListener('click', function (e) {
                _this.toggleTabs(e);
                _this.toggleContent(e);
            });
        });
    };
    Tabs.prototype.toggleTabs = function (e) {
        var clickedTab = e.currentTarget;
        this.tabs.forEach(function (tab) {
            tab.classList.remove('active');
            tab.setAttribute('aria-selected', 'false');
        });
        clickedTab.classList.add('active');
        clickedTab.setAttribute('aria-selected', 'true');
    };
    Tabs.prototype.toggleContent = function (e) {
        var clickedTab = e.currentTarget;
        var selector = clickedTab.dataset.tab;
        var selectedContent = Array.from(this.contents).find(function (content) { return content.dataset.content === selector; });
        if (selectedContent) {
            this.contents.forEach(function (content) {
                content.classList.remove('active');
                content.setAttribute('aria-hidden', 'true');
            });
            selectedContent.classList.add('active');
            selectedContent.setAttribute('aria-hidden', 'false');
        }
    };
    return Tabs;
}());
