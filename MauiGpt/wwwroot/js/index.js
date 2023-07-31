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

getSelectionText = () => {
    var text = "";
    if (window.getSelection) {
        text = window.getSelection().toString();
        console.log("1:", text);
    } else if (document.selection && document.selection.type != "Control") {
        text = document.selection.createRange().text;
        console.log("2:", text);
    }
    return text;
}

var contextMenu = CtxMenu();

// Add an item to the menu
contextMenu.addItem("Copy", function () {
    document.execCommand("copy");
});