var lastScrollHeight = -1;

scrollListToEnd = (element) => {
    if (element.scrollHeight !== lastScrollHeight) {
        lastScrollHeight = element.scrollHeight;
        element.scrollTop = element.scrollHeight;
    }
}

colorCodeblock = (htmlContent) => {
    return hljs.highlightAuto(htmlContent).value;
};

colorAllCode = () => {
    hljs.highlightAll();
};

function copyToClipboard(id) {
    var r = document.createRange();
    r.selectNode(document.getElementById(id));
    window.getSelection().removeAllRanges();
    window.getSelection().addRange(r);
    document.execCommand('copy');
    window.getSelection().removeAllRanges();
}

var contextMenu = CtxMenu();

// Add an item to the menu
contextMenu.addItem("Copy", function () {
    document.execCommand("copy");
});