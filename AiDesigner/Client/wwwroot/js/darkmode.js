function toggleDarkMode() {
    document.documentElement.classList.toggle('dark');
}

window.getTitle = () => {
    return document.title;
};

//function toggleDarkMode() {
//    var htmlElement = document.documentElement;

//    if (htmlElement.classList.contains('dark')) {
//        htmlElement.classList.remove('dark');
//        htmlElement.classList.add('light');
//    } else {
//        htmlElement.classList.remove('light');
//        htmlElement.classList.add('dark');
//    }
//}