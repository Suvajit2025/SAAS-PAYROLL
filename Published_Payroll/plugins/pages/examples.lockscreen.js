/*
Name: 			Pages / Locked Screen - Examples
Written by: 	Clara Garnier - claragarnier.com
Theme Version: 	1.0.0 - TTMS
*/

(function($) {

	'use strict';

	var $document,
		idleTime;

	$document = $(document);

	$(function() {
		$.idleTimer( 10000 ); // ms

		$document.on( 'idle.idleTimer', function() {
			// if you don't want the modal
			// you can make a redirect here
			LockScreen.show();
		});
	});

}).apply( this, [ jQuery ]);
