/*
Name: 			UI Elements / Portlets - Examples
Written by: 	Clara Garnier - claragarnier.com
Theme Version: 	1.0.0 - TTMS
*/

(function( $ ) {

	'use strict';

	/*
	Refresh page
	*/
	$('#portletRefresh').on('click', function(ev) {
		ev.preventDefault();
		window.location.reload();
	});

	/*
	Restore to default
	*/
	$('#portletReset').on('click', function(ev) {
		ev.preventDefault();
		store.remove('__portletOrder');
		store.remove('__portletState');
		window.location.reload();
	});



}).apply( this, [ jQuery ]);
