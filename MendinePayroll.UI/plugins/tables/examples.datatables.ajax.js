/*
Name: 			Tables / Ajax - Examples
Written by: 	Clara Garnier - claragarnier.com
Theme Version: 	1.0.0 - TTMS
*/

(function( $ ) {

	'use strict';

	var datatableInit = function() {

		var $table = $('#datatable-ajax');
		$table.dataTable({
			bProcessing: true,
			sAjaxSource: $table.data('url')
		});

	};

	$(function() {
		datatableInit();
	});

}).apply( this, [ jQuery ]);
