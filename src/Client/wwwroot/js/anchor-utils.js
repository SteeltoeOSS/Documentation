function scrollToAnchor(anchor) {
	var selector = anchor || document.location.hash;
	
    if (selector && selector.length > 1) {
			var element = document.querySelector(selector);
			//console.info(element);
        if (element) {
            var y = element.getBoundingClientRect().top + window.pageYOffset;
						//y -= document.querySelector("#main").offsetHeight;
					
							window.scroll(0, y);
        }
    }
    else
        window.scroll(0, 0);
}