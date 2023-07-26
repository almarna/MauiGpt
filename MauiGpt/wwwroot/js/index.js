function scrollToEnd(element) {
    element.scrollTop = element.scrollHeight;
}

colorCodeblock = (htmlContent) => {
    return hljs.highlightAuto(htmlContent).value;
};

colorAllCode = () => {
    hljs.highlightAll();
};