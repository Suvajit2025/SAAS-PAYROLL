$(function () {
	MenuList();

    function MenuList() {
        let url = "/Dashboard/MenuList";
        $.get(url, function (data) {
            //console.log(data);
            let main = data[0].MainMenu;
            let sub = data[0].SubMenu;
            let txt = '';
            //txt += '<hr class="separator" />';
            let mainID = 0;
            console.log(main);
            console.log(sub);
            $.each(main, function () {
                mainID = this.MenuSRL;
                txt += '<li class="nav-parent">';
                txt += '<a><i class="fa fa-database"></i>' + this.MainMenu + '</a>'
                txt += '<ul class="nav nav-children">';
                let submenu = sub.filter(item => item.MenuSRL == mainID);
                // Sub Menu Loop
                $.each(submenu, function () {
                    txt += '<li>';
                    txt += '<a href="' + this.MenuURL + '"><i class="' + this.MenuIcon + '" aria-hidden="true"></i><span>' + this.SubMenu + '</span></a>';
                    txt += '</li>';
                })
                txt += '<hr class="separator" />';
                txt += '</ul>';
                txt += '</li>';
                $("#divMenu").append(txt);
                txt = '';
            })
        })
    }





	//var $items = $('.nav-main li.nav-parent');

	function expand($li) {
		//alert("test");
		$li.children('ul.nav-children').slideDown('fast', function () {
			$li.addClass('nav-expanded');
			$(this).css('display', '');
			ensureVisible($li);
		});
	}

	function collapse($li) {
		$li.children('ul.nav-children').slideUp('fast', function () {
			$(this).css('display', '');
			$li.removeClass('nav-expanded');
		});
	}

	function ensureVisible($li) {
		var scroller = $li.offsetParent();
		if (!scroller.get(0)) {
			return false;
		}

		var top = $li.position().top;
		if (top < 0) {
			scroller.animate({
				scrollTop: scroller.scrollTop() + top
			}, 'fast');
		}
	}

	$('.nav.nav-main').on('click', 'li.nav-parent', function (ev) {
		//alert("test");
		var $anchor = $(this),
			$prev = $anchor.closest('ul.nav').find('> li.nav-expanded'),
			$next = $anchor.closest('li');

		if ($anchor.prop('href')) {
			var arrowWidth = parseInt(window.getComputedStyle($anchor.get(0), ':after').width, 10) || 0;
			if (ev.offsetX > $anchor.get(0).offsetWidth - arrowWidth) {
				ev.preventDefault();
			}
		}

		if ($prev.get(0) !== $next.get(0)) {
			collapse($prev);
			expand($next);
		} else {
			collapse($prev);
		}
	});
});


