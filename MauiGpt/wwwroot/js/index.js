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